using Boxty.ServerApp.Modules.Settings.Entities;
using Boxty.ServerApp.Modules.Settings.Infrastructure.Database;
using Boxty.SharedApp.DTOs.Settings;
using Boxty.SharedApp.Validators;
using Microsoft.EntityFrameworkCore;

namespace Boxty.Tests;

public class SmtpDefaultsTests
{
    [Fact]
    public async Task EmptyDatabaseSeedsFormerAppsettingsValuesWithAuditFields()
    {
        await using var db = CreateDatabase();

        SettingsSeedData.Seed(db);

        var seed = await db.SmtpConfigs.SingleAsync();
        Assert.Equal("Default SMTP Configuration", seed.Name);
        Assert.Equal("info@boxty.com", seed.SenderAddress);
        Assert.Equal("Boxty System", seed.SenderName);
        Assert.Equal("smtp.ionos.co.uk", seed.SmtpHost);
        Assert.Equal(465, seed.SmtpPort);
        Assert.Equal("emailsmtp@boxty.com", seed.SmtpUsername);
        Assert.Equal("YOUR-SMTP-PASSWORD", seed.SmtpPassword);
        Assert.True(seed.EnableEmailSending);
        Assert.True(seed.IsActive);
        Assert.NotEqual(Guid.Empty, seed.Id);
        Assert.Equal("System", seed.CreatedBy);
        Assert.Equal("System", seed.LastModifiedBy);
        Assert.Equal(DateTimeKind.Utc, seed.CreatedDate.Kind);
        Assert.Equal(seed.CreatedDate, seed.ModifiedDate);
        Assert.Equal(new Guid("c1e30e05-0655-42b6-9e4f-32310eb650c8"), seed.TenantId);
        Assert.Equal(new Guid("de8d2617-58f1-4965-a523-811e2f1a1eec"), seed.SubjectId);
        Assert.Equal(seed.SubjectId, seed.CreatedById);
        Assert.Equal(seed.SubjectId, seed.ModifiedById);
    }

    [Fact]
    public async Task RepeatedSeedDoesNotDuplicateOrOverwriteEditedSeed()
    {
        await using var db = CreateDatabase();
        SettingsSeedData.Seed(db);
        var seed = await db.SmtpConfigs.SingleAsync();
        var id = seed.Id;
        seed.SmtpHost = "edited.example.com";
        seed.SmtpPassword = "test-edited-password";
        await db.SaveChangesAsync();

        SettingsSeedData.Seed(db);

        var saved = await db.SmtpConfigs.SingleAsync();
        Assert.Equal(id, saved.Id);
        Assert.Equal("edited.example.com", saved.SmtpHost);
        Assert.Equal("test-edited-password", saved.SmtpPassword);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ExistingConfigurationPreventsSeedingEvenWhenArchived(bool isActive)
    {
        await using var db = CreateDatabase();
        var record = new SmtpConfig
        {
            Id = Guid.NewGuid(),
            Name = "Existing",
            SenderAddress = "existing@example.com",
            SenderName = "Existing Sender",
            SmtpHost = "existing.example.com",
            SmtpPort = 587,
            SmtpUsername = "existing-user",
            SmtpPassword = "test-existing-password",
            IsActive = isActive
        };
        db.SmtpConfigs.Add(record);
        await db.SaveChangesAsync();

        SettingsSeedData.Seed(db);

        Assert.Equal(record.Id, (await db.SmtpConfigs.SingleAsync()).Id);
        Assert.Equal("existing.example.com", record.SmtpHost);
        Assert.Equal(EntityState.Unchanged, db.Entry(record).State);
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(65535, true)]
    [InlineData(65536, false)]
    public void ValidatorEnforcesPortRange(int port, bool valid)
    {
        var dto = ValidDto();
        dto.SmtpPort = port;
        Assert.Equal(valid, new SmtpConfigValidator().Validate(dto).IsValid);
    }

    [Fact]
    public void ValidatorRejectsInvalidEmailAndMissingRequiredFields()
    {
        var dto = ValidDto();
        dto.SenderAddress = "not-an-email";
        dto.Name = "";
        dto.SmtpHost = "";
        dto.SmtpUsername = "";
        dto.SmtpPassword = "";
        var errors = new SmtpConfigValidator().Validate(dto).Errors.Select(error => error.PropertyName);
        Assert.Contains(nameof(dto.SenderAddress), errors);
        Assert.Contains(nameof(dto.Name), errors);
        Assert.Contains(nameof(dto.SmtpHost), errors);
        Assert.Contains(nameof(dto.SmtpUsername), errors);
        Assert.Contains(nameof(dto.SmtpPassword), errors);
    }

    internal static SmtpConfigDto ValidDto() => new()
    {
        Name = "Test SMTP",
        SenderAddress = "sender@example.com",
        SenderName = "Sender",
        SmtpHost = "smtp.example.com",
        SmtpPort = 587,
        SmtpUsername = "test-user",
        SmtpPassword = "test-only-password",
        EnableEmailSending = true
    };

    private static SettingsDbContext CreateDatabase() => new(new DbContextOptionsBuilder<SettingsDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
}
