using System.Security.Claims;
using Boxty.ServerApp.Modules.UserManagement.Services;
using FS.Keycloak.RestApiClient.Model;

namespace Boxty.ServerApp.Modules.UserManagement.Infrastructure.Commands
{
    public interface IRemoveUserRoleCommand
    {
        Task<bool> Handle(Guid userId, List<string> roleNames, ClaimsPrincipal user);
    }

    public class RemoveUserRoleCommand : IRemoveUserRoleCommand
    {
        private readonly IKeycloakService _keycloakService;

        public RemoveUserRoleCommand(IKeycloakService keycloakService)
        {
            _keycloakService = keycloakService;
        }

        public async Task<bool> Handle(Guid userId, List<string> roleNames, ClaimsPrincipal user)
        {
            try
            {
                await KeycloakAdminCommandHelper.EnsureUserExistsAsync(_keycloakService, userId);

                var currentRoles = await _keycloakService.GetUserRolesAsync(userId.ToString());

                var rolesToRemove = new List<RoleRepresentation>();
                foreach (var roleName in roleNames)
                {
                    var normalizedRoleName = KeycloakAdminCommandHelper.NormalizeRoleName(roleName);
                    var role = currentRoles.FirstOrDefault(r =>
                        r.Name?.Equals(normalizedRoleName, StringComparison.OrdinalIgnoreCase) ?? false);

                    if (role == null)
                    {
                        throw new InvalidOperationException($"User does not have role '{normalizedRoleName}'.");
                    }

                    rolesToRemove.Add(role);
                }

                await _keycloakService.DeleteUserRoleMappingAsync(userId.ToString(), rolesToRemove);

                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to remove roles from user: {ex.Message}", ex);
            }
        }
    }
}