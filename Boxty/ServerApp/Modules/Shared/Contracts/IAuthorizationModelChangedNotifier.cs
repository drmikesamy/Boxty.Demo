namespace Boxty.ServerApp.Modules.Shared.Contracts
{
    public interface IAuthorizationModelChangedNotifier
    {
        Task NotifyChangedAsync();
    }
}