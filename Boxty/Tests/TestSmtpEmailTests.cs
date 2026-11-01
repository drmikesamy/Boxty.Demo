using Boxty.ServerBase.Config;
using Boxty.ServerBase.Services;
using Boxty.ServerApp.Modules.Settings.Endpoints;
using Boxty.ServerApp.Modules.Settings.Infrastructure.Commands;
using Boxty.SharedApp.DTOs.Settings;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Boxty.Tests;

public class TestSmtpEmailTests
{
    [Fact]
    public async Task TestEndpointRequiresSmtpUpdatePermission()
    {
        var builder = WebApplication.CreateBuilder();
        await using var app = builder.Build();
        new SmtpConfigEndpoints().MapEndpoints(app);
        var endpoint = ((IEndpointRouteBuilder)app).DataSources.SelectMany(source => source.Endpoints)
            .OfType<RouteEndpoint>().Single(e => e.RoutePattern.RawText == "/api/SmtpConfig/TestEmail");

        Assert.Contains(endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>(),
            authorization => authorization.Policy == "Permission:UpdateSmtpConfig");
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public async Task InvalidSenderIsRejectedBeforeConnecting(string sender)
    {
        var options = new Mock<IOptions<AppOptions>>(MockBehavior.Strict);
        var command = new TestSmtpEmailCommand(options.Object, NullLogger<EmailService>.Instance);
        var settings = SmtpDefaultsTests.ValidDto();
        settings.SenderAddress = sender;

        await Assert.ThrowsAsync<ValidationException>(() =>
            command.Handle(new TestSmtpEmailRequest(settings), CancellationToken.None));

        options.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task InvalidSmtpSettingsAreRejectedBeforeConnecting()
    {
        var dto = SmtpDefaultsTests.ValidDto();
        dto.SmtpPort = 0;
        var options = new Mock<IOptions<AppOptions>>(MockBehavior.Strict);
        var command = new TestSmtpEmailCommand(options.Object, NullLogger<EmailService>.Instance);

        await Assert.ThrowsAsync<ValidationException>(() =>
            command.Handle(new TestSmtpEmailRequest(dto), CancellationToken.None));
        options.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task EndpointDispatchesCommandWithRequestAndCancellation()
    {
        var request = new TestSmtpEmailRequest(SmtpDefaultsTests.ValidDto());
        using var cancellation = new CancellationTokenSource();
        var command = new Mock<ITestSmtpEmailCommand>();
        command.Setup(c => c.Handle(request, cancellation.Token)).Returns(Task.CompletedTask);

        var result = await new TestEndpoints().Send(command.Object, request, cancellation.Token);

        Assert.IsType<NoContent>(result);
        command.Verify(c => c.Handle(request, cancellation.Token), Times.Once());
    }

    [Fact]
    public async Task EndpointDoesNotExposeCredentialsOnFailure()
    {
        var request = new TestSmtpEmailRequest(SmtpDefaultsTests.ValidDto());
        var command = new Mock<ITestSmtpEmailCommand>();
        command.Setup(c => c.Handle(request, CancellationToken.None))
            .ThrowsAsync(new InvalidOperationException(request.Settings.SmtpPassword));

        var result = await new TestEndpoints().Send(command.Object, request, CancellationToken.None);

        var problem = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(500, problem.StatusCode);
        Assert.DoesNotContain(request.Settings.SmtpPassword, problem.ProblemDetails.Detail!);
    }

    private sealed class TestEndpoints : SmtpConfigEndpoints
    {
        public Task<IResult> Send(ITestSmtpEmailCommand command, TestSmtpEmailRequest request, CancellationToken token) =>
            TestEmail(command, request, token);
    }
}
