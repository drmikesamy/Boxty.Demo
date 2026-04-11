using Boxty.ServerApp.Modules.Shared.Contracts;
using Boxty.ServerBase.Services;

namespace Boxty.ServerApp.Modules.UserManagement.Infrastructure
{
    internal sealed class AuthorizationModelChangedNotifier : IAuthorizationModelChangedNotifier
    {
        private readonly IRolePermissionCacheService _rolePermissionCacheService;

        public AuthorizationModelChangedNotifier(IRolePermissionCacheService rolePermissionCacheService)
        {
            _rolePermissionCacheService = rolePermissionCacheService;
        }

        public Task NotifyChangedAsync()
        {
            return _rolePermissionCacheService.InitAsync();
        }
    }
}