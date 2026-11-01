using Boxty.ServerBase.Config;
using Boxty.ServerBase.Interfaces;
using Boxty.ServerBase.Services;
using Boxty.SharedApp.DTOs.Settings;
using Boxty.SharedApp.Validators;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Boxty.ServerApp.Modules.Settings.Infrastructure.Commands;

public interface ITestSmtpEmailCommand
{
    Task Handle(TestSmtpEmailRequest request, CancellationToken cancellationToken);
}

public class TestSmtpEmailCommand(IOptions<AppOptions> appOptions, ILogger<EmailService> logger) : ITestSmtpEmailCommand, ICommand
{
    public async Task Handle(TestSmtpEmailRequest request, CancellationToken cancellationToken)
    {
        var validator = new InlineValidator<TestSmtpEmailRequest>();
        validator.RuleFor(x => x.Settings).NotNull().SetValidator(new SmtpConfigValidator());
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var settings = request.Settings;
        var service = new EmailService(Options.Create(new AppOptions
        {
            ConnectionStrings = appOptions.Value.ConnectionStrings,
            Logging = appOptions.Value.Logging,
            Cors = appOptions.Value.Cors,
            AllowedHosts = appOptions.Value.AllowedHosts,
            Jwt = appOptions.Value.Jwt,
            KeycloakAdminClient = appOptions.Value.KeycloakAdminClient,
            AuthModule = appOptions.Value.AuthModule,
            Modules = appOptions.Value.Modules,
            Email = new EmailOptions
            {
                SenderAddress = settings.SenderAddress,
                SenderName = settings.SenderName,
                SmtpHost = settings.SmtpHost,
                SmtpPort = settings.SmtpPort,
                SmtpUsername = settings.SmtpUsername,
                SmtpPassword = settings.SmtpPassword,
                EnableEmailSending = true
            }
        }), logger);
        var result = await service.SendEmailAsync(settings.SenderAddress, settings.SenderAddress,
            "Boxty SMTP test", "<p>Your Boxty SMTP settings successfully sent this test email.</p>",
            "Your Boxty SMTP settings successfully sent this test email.", cancellationToken: cancellationToken);
        if (!result.Success)
        {
            logger.LogError("SMTP test did not complete successfully.");
            throw new InvalidOperationException("SMTP test failed.");
        }
    }
}
