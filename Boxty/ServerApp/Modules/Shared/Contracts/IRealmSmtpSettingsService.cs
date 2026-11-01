namespace Boxty.ServerApp.Modules.Shared.Contracts;

public interface IRealmSmtpSettingsService
{
    Task UpdateAsync(Dictionary<string, string> settings, CancellationToken cancellationToken);
}
