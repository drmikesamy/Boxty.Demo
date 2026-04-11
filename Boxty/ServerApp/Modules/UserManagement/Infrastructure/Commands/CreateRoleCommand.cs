using System.Security.Claims;
using Boxty.ServerApp.Modules.UserManagement.Services;
using FS.Keycloak.RestApiClient.Model;

namespace Boxty.ServerApp.Modules.UserManagement.Infrastructure.Commands
{
    public interface ICreateRoleCommand
    {
        Task<bool> Handle(string roleName, string? description, ClaimsPrincipal user);
    }

    public class CreateRoleCommand : ICreateRoleCommand
    {
        private readonly IKeycloakService _keycloakService;

        public CreateRoleCommand(IKeycloakService keycloakService)
        {
            _keycloakService = keycloakService;
        }

        public async Task<bool> Handle(string roleName, string? description, ClaimsPrincipal user)
        {
            try
            {
                var normalizedRoleName = KeycloakAdminCommandHelper.NormalizeRoleName(roleName);

                if (await RoleExistsAsync(normalizedRoleName))
                {
                    throw new InvalidOperationException($"Role '{normalizedRoleName}' already exists.");
                }

                var newRole = new RoleRepresentation
                {
                    Name = normalizedRoleName,
                    Description = description
                };

                await _keycloakService.CreateRoleAsync(newRole);

                return true;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to create role ({ex.GetType().Name}): {ex.Message}", ex);
            }
        }

        private async Task<bool> RoleExistsAsync(string normalizedRoleName)
        {
            try
            {
                var existingRole = await _keycloakService.GetRoleByNameAsync(normalizedRoleName);
                return existingRole != null;
            }
            catch (Exception ex) when (KeycloakAdminCommandHelper.IsNotFound(ex))
            {
                return false;
            }
        }
    }
}