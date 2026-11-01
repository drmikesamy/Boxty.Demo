using System.Globalization;
using System.Security.Claims;
using Boxty.ServerBase.Commands;
using Boxty.ServerBase.Interfaces;
using Boxty.ServerApp.Modules.Settings.Entities;
using Boxty.ServerApp.Modules.Settings.Infrastructure.Database;
using Boxty.ServerApp.Modules.Shared.Contracts;
using Boxty.SharedApp.DTOs.Settings;
using Microsoft.Extensions.Logging;

namespace Boxty.ServerApp.Modules.Settings.Infrastructure.Commands;

public interface ISaveSmtpConfigCommand
{
    Task<Guid> Create(SmtpConfigDto dto, ClaimsPrincipal user, CancellationToken cancellationToken);
    Task<Guid> Update(SmtpConfigDto dto, ClaimsPrincipal user, CancellationToken cancellationToken);
}

public class SaveSmtpConfigCommand(
    SettingsDbContext dbContext,
    ICreateCommand<SmtpConfig, SmtpConfigDto, SettingsDbContext> createCommand,
    IUpdateCommand<SmtpConfig, SmtpConfigDto, SettingsDbContext> updateCommand,
    IRealmSmtpSettingsService realmSmtpSettingsService,
    ILogger<SaveSmtpConfigCommand> logger) : ISaveSmtpConfigCommand, ICommand
{
    public Task<Guid> Create(SmtpConfigDto dto, ClaimsPrincipal user, CancellationToken cancellationToken) =>
        SaveAndSynchronize(() => createCommand.Handle(dto, user), dto, cancellationToken);

    public Task<Guid> Update(SmtpConfigDto dto, ClaimsPrincipal user, CancellationToken cancellationToken) =>
        SaveAndSynchronize(() => updateCommand.Handle(dto, user), dto, cancellationToken);

    private async Task<Guid> SaveAndSynchronize(
        Func<Task<Guid>> save, SmtpConfigDto dto, CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var keycloakUpdated = false;
        try
        {
            var id = await save();
            await realmSmtpSettingsService.UpdateAsync(ToKeycloakSettings(dto), cancellationToken);
            keycloakUpdated = true;
            await transaction.CommitAsync(cancellationToken);
            return id;
        }
        catch (Exception)
        {
            if (keycloakUpdated)
            {
                logger.LogCritical("SMTP database commit failed after Keycloak was updated. Verify both configurations before retrying.");
            }
            else
            {
                logger.LogError("SMTP save or Keycloak synchronization failed. The database transaction has not been committed.");
            }
            throw;
        }
    }

    internal static Dictionary<string, string> ToKeycloakSettings(SmtpConfigDto dto) => new()
    {
        ["from"] = dto.SenderAddress,
        ["fromDisplayName"] = dto.SenderName,
        ["host"] = dto.SmtpHost,
        ["port"] = dto.SmtpPort.ToString(CultureInfo.InvariantCulture),
        ["auth"] = "true",
        ["user"] = dto.SmtpUsername,
        ["password"] = dto.SmtpPassword,
        ["ssl"] = dto.SmtpPort == 465 ? "true" : "false",
        ["starttls"] = dto.SmtpPort == 465 ? "false" : "true"
    };
}
