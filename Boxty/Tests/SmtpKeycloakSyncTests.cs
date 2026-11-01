using System.Security.Claims;
using Boxty.ServerBase.Commands;
using Boxty.ServerApp.Modules.Settings.Endpoints;
using Boxty.ServerApp.Modules.Settings.Entities;
using Boxty.ServerApp.Modules.Settings.Infrastructure.Database;
using Boxty.ServerApp.Modules.Shared.Contracts;
using Boxty.SharedApp.DTOs.Settings;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using FS.Keycloak.RestApiClient.Api;
using FS.Keycloak.RestApiClient.Model;
using Boxty.ServerApp.Modules.UserManagement.Services;
using Boxty.ServerApp.Modules.Settings.Infrastructure.Commands;
using Microsoft.Extensions.Logging.Abstractions;

namespace Boxty.Tests;

public class SmtpKeycloakSyncTests
{
    [Fact]
    public async Task RealmUpdatePreservesOtherSmtpOptionsAndOnlySendsSmtpSettings()
    {
        var api = new Mock<IRealmsAdminApi>();
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        api.Setup(client => client.GetAsync("boxty", token)).ReturnsAsync(new RealmRepresentation
        {
            Realm = "boxty",
            Enabled = true,
            SmtpServer = new Dictionary<string, string>
            {
                ["replyTo"] = "support@example.com",
                ["password"] = "**********",
                ["host"] = "old.example.com"
            }
        });
        api.Setup(client => client.PutAsync("boxty", It.IsAny<RealmRepresentation>(), token)).Returns(Task.CompletedTask);
        var settings = SaveSmtpConfigCommand.ToKeycloakSettings(SmtpDefaultsTests.ValidDto());

        await KeycloakService.UpdateRealmSmtpAsync(api.Object, "boxty", settings, token);

        api.Verify(client => client.PutAsync("boxty", It.Is<RealmRepresentation>(realm =>
            realm.SmtpServer["replyTo"] == "support@example.com"
            && realm.SmtpServer["password"] == settings["password"]
            && realm.SmtpServer["host"] == settings["host"]
            && realm.Realm == null && realm.Enabled == null), token), Times.Once());
    }

    [Theory]
    [InlineData(465, "true", "false")]
    [InlineData(587, "false", "true")]
    [InlineData(25, "false", "true")]
    public void MapsSmtpFieldsAndTransportWithoutBoxtyEnableToggle(int port, string ssl, string startTls)
    {
        var dto = SmtpDefaultsTests.ValidDto();
        dto.SmtpPort = port;
        dto.EnableEmailSending = false;

        var settings = SaveSmtpConfigCommand.ToKeycloakSettings(dto);

        Assert.Equal(dto.SenderAddress, settings["from"]);
        Assert.Equal(dto.SenderName, settings["fromDisplayName"]);
        Assert.Equal(dto.SmtpHost, settings["host"]);
        Assert.Equal(port.ToString(), settings["port"]);
        Assert.Equal(dto.SmtpUsername, settings["user"]);
        Assert.Equal(dto.SmtpPassword, settings["password"]);
        Assert.Equal("true", settings["auth"]);
        Assert.Equal(ssl, settings["ssl"]);
        Assert.Equal(startTls, settings["starttls"]);
        Assert.Equal(settings, SaveSmtpConfigCommand.ToKeycloakSettings(new SmtpConfigDto
        {
            SenderAddress = dto.SenderAddress,
            SenderName = dto.SenderName,
            SmtpHost = dto.SmtpHost,
            SmtpPort = dto.SmtpPort,
            SmtpUsername = dto.SmtpUsername,
            SmtpPassword = dto.SmtpPassword,
            EnableEmailSending = true
        }));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SuccessfulSaveSynchronizesBeforeCommitting(bool update)
    {
        await using var fixture = await Fixture.Create();
        var dto = SmtpDefaultsTests.ValidDto();
        dto.Id = Guid.NewGuid();
        if (update)
        {
            fixture.Database.SmtpConfigs.Add(Entity(dto.Id, "before.example.com"));
            await fixture.Database.SaveChangesAsync();
            fixture.Database.ChangeTracker.Clear();
        }
        fixture.Sync.Setup(service => service.UpdateAsync(It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .Callback(() => Assert.NotNull(fixture.Database.Database.CurrentTransaction))
            .Returns(Task.CompletedTask);

        var result = await fixture.Save(dto, update);

        Assert.Equal(dto.Id, Assert.IsType<Ok<Guid>>(result).Value);
        Assert.Equal(dto.SmtpHost, (await fixture.Database.SmtpConfigs.AsNoTracking().SingleAsync()).SmtpHost);
        fixture.Sync.Verify(service => service.UpdateAsync(
            It.Is<Dictionary<string, string>>(settings => settings["host"] == dto.SmtpHost && settings["password"] == dto.SmtpPassword),
            It.IsAny<CancellationToken>()), Times.Once());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task KeycloakFailureRollsBackCreateOrUpdate(bool update)
    {
        await using var fixture = await Fixture.Create();
        var dto = SmtpDefaultsTests.ValidDto();
        dto.Id = Guid.NewGuid();
        if (update)
        {
            fixture.Database.SmtpConfigs.Add(Entity(dto.Id, "before.example.com"));
            await fixture.Database.SaveChangesAsync();
            fixture.Database.ChangeTracker.Clear();
        }
        fixture.Sync.Setup(service => service.UpdateAsync(It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Keycloak unavailable"));

        var result = await fixture.Save(dto, update);

        var problem = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(500, problem.StatusCode);
        Assert.Contains("Keycloak", problem.ProblemDetails.Detail);
        fixture.Database.ChangeTracker.Clear();
        var stored = await fixture.Database.SmtpConfigs.AsNoTracking().SingleOrDefaultAsync();
        if (update)
        {
            Assert.NotNull(stored);
            Assert.Equal("before.example.com", stored.SmtpHost);
        }
        else
        {
            Assert.Null(stored);
        }
    }

    [Fact]
    public async Task ValidationFailureDoesNotCallKeycloak()
    {
        await using var fixture = await Fixture.Create();
        var command = new Mock<IUpdateCommand<SmtpConfig, SmtpConfigDto, SettingsDbContext>>();
        command.Setup(c => c.Handle(It.IsAny<SmtpConfigDto>(), It.IsAny<ClaimsPrincipal>()))
            .ThrowsAsync(new ValidationException("Invalid settings"));

        var saveCommand = new SaveSmtpConfigCommand(fixture.Database,
            Mock.Of<ICreateCommand<SmtpConfig, SmtpConfigDto, SettingsDbContext>>(),
            command.Object, fixture.Sync.Object, NullLogger<SaveSmtpConfigCommand>.Instance);
        var result = await new TestEndpoints().UpdateSettings(saveCommand, SmtpDefaultsTests.ValidDto());

        Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(400, ((IStatusCodeHttpResult)result).StatusCode);
        fixture.Sync.VerifyNoOtherCalls();
    }

    private sealed class TestEndpoints : SmtpConfigEndpoints
    {
        public Task<IResult> CreateSettings(ISaveSmtpConfigCommand command,
            SmtpConfigDto dto) => Create(command, new ClaimsPrincipal(), dto, CancellationToken.None);

        public Task<IResult> UpdateSettings(ISaveSmtpConfigCommand command,
            SmtpConfigDto dto) => Update(command, new ClaimsPrincipal(), dto, CancellationToken.None);
    }

    private static SmtpConfig Entity(Guid id, string host) => new()
    {
        Id = id,
        Name = "Test",
        SenderAddress = "sender@example.com",
        SenderName = "Sender",
        SmtpHost = host,
        SmtpPort = 587,
        SmtpUsername = "test-user",
        SmtpPassword = "test-only-password"
    };

    private sealed class Fixture : IAsyncDisposable
    {
        private readonly SqliteConnection connection;
        public SettingsDbContext Database { get; }
        public Mock<IRealmSmtpSettingsService> Sync { get; } = new();

        private Fixture(SqliteConnection connection)
        {
            this.connection = connection;
            Database = new SettingsDbContext(new DbContextOptionsBuilder<SettingsDbContext>().UseSqlite(connection).Options);
        }

        public static async Task<Fixture> Create()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var fixture = new Fixture(connection);
            await fixture.Database.Database.EnsureCreatedAsync();
            return fixture;
        }

        public async Task<IResult> Save(SmtpConfigDto dto, bool update)
        {
            async Task<Guid> Save()
            {
                var entity = await Database.SmtpConfigs.FindAsync(dto.Id);
                if (entity == null)
                {
                    entity = Entity(dto.Id, dto.SmtpHost);
                    Database.SmtpConfigs.Add(entity);
                }
                entity.Name = dto.Name;
                entity.SmtpHost = dto.SmtpHost;
                await Database.SaveChangesAsync();
                return entity.Id;
            }
            var endpoints = new TestEndpoints();
            var updateCommand = new Mock<IUpdateCommand<SmtpConfig, SmtpConfigDto, SettingsDbContext>>();
            updateCommand.Setup(c => c.Handle(dto, It.IsAny<ClaimsPrincipal>())).Returns(Save);
            var createCommand = new Mock<ICreateCommand<SmtpConfig, SmtpConfigDto, SettingsDbContext>>();
            createCommand.Setup(c => c.Handle(dto, It.IsAny<ClaimsPrincipal>(), false)).Returns(Save);
            var command = new SaveSmtpConfigCommand(Database, createCommand.Object, updateCommand.Object,
                Sync.Object, NullLogger<SaveSmtpConfigCommand>.Instance);
            return update
                ? await endpoints.UpdateSettings(command, dto)
                : await endpoints.CreateSettings(command, dto);
        }

        public async ValueTask DisposeAsync()
        {
            await Database.DisposeAsync();
            await connection.DisposeAsync();
        }
    }
}
