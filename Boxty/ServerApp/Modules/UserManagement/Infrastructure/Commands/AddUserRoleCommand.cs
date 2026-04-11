using System.Security.Claims;
using Boxty.ServerApp.Modules.UserManagement.Services;
using FS.Keycloak.RestApiClient.Model;

namespace Boxty.ServerApp.Modules.UserManagement.Infrastructure.Commands
{
    public interface IAddUserRoleCommand
    {
        Task<bool> Handle(Guid userId, List<string> roleNames, ClaimsPrincipal user);
    }

    public class AddUserRoleCommand : IAddUserRoleCommand
    {
        private readonly IKeycloakService _keycloakService;

        public AddUserRoleCommand(IKeycloakService keycloakService)
        {
            _keycloakService = keycloakService;
        }

        public async Task<bool> Handle(Guid userId, List<string> roleNames, ClaimsPrincipal user)
        {
            try
            {
                await KeycloakAdminCommandHelper.EnsureUserExistsAsync(_keycloakService, userId);
                var roles = await KeycloakAdminCommandHelper.GetRequiredRolesAsync(_keycloakService, roleNames);

                await _keycloakService.PostUserRoleMappingAsync(userId.ToString(), roles);

                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to add roles to user: {ex.Message}", ex);
            }
        }
    }
}