using System.Security.Claims;
using Boxty.ServerApp.Modules.UserManagement.Services;
using FS.Keycloak.RestApiClient.Model;

namespace Boxty.ServerApp.Modules.UserManagement.Infrastructure.Commands
{
    public interface IUpdateUserRolesCommand
    {
        Task<bool> Handle(Guid userId, List<string> roleNames, ClaimsPrincipal user);
    }

    public class UpdateUserRolesCommand : IUpdateUserRolesCommand
    {
        private readonly IKeycloakService _keycloakService;

        public UpdateUserRolesCommand(IKeycloakService keycloakService)
        {
            _keycloakService = keycloakService;
        }

        public async Task<bool> Handle(Guid userId, List<string> roleNames, ClaimsPrincipal user)
        {
            try
            {
                await KeycloakAdminCommandHelper.EnsureUserExistsAsync(_keycloakService, userId);

                var currentRoles = await _keycloakService.GetUserRolesAsync(userId.ToString());

                if (currentRoles.Any())
                {
                    await _keycloakService.DeleteUserRoleMappingAsync(userId.ToString(), currentRoles);
                }

                if (roleNames.Any())
                {
                    var newRoles = await KeycloakAdminCommandHelper.GetRequiredRolesAsync(_keycloakService, roleNames);
                    await _keycloakService.PostUserRoleMappingAsync(userId.ToString(), newRoles);
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to update user roles: {ex.Message}", ex);
            }
        }
    }
}