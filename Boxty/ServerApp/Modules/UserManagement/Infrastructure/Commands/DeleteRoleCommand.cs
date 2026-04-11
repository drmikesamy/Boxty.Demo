using System.Security.Claims;
using Boxty.ServerApp.Modules.UserManagement.Services;

namespace Boxty.ServerApp.Modules.UserManagement.Infrastructure.Commands
{
    public interface IDeleteRoleCommand
    {
        Task<bool> Handle(string roleName, ClaimsPrincipal user);
    }

    public class DeleteRoleCommand : IDeleteRoleCommand
    {
        private readonly IKeycloakService _keycloakService;

        public DeleteRoleCommand(IKeycloakService keycloakService)
        {
            _keycloakService = keycloakService;
        }

        public async Task<bool> Handle(string roleName, ClaimsPrincipal user)
        {
            try
            {
                var normalizedRoleName = KeycloakAdminCommandHelper.NormalizeRoleName(roleName);
                var existingRole = await _keycloakService.GetRoleByNameAsync(normalizedRoleName);
                if (existingRole == null)
                {
                    throw new InvalidOperationException($"Role '{normalizedRoleName}' not found.");
                }

                await _keycloakService.DeleteRoleAsync(normalizedRoleName);

                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to delete role: {ex.Message}", ex);
            }
        }
    }
}