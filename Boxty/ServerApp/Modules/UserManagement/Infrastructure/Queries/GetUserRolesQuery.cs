using System.Security.Claims;
using Boxty.ServerApp.Modules.UserManagement.Services;
using FS.Keycloak.RestApiClient.Model;

namespace Boxty.ServerApp.Modules.UserManagement.Infrastructure.Queries
{
    public interface IGetUserRolesQuery
    {
        Task<ICollection<RoleRepresentation>> Handle(Guid userId, ClaimsPrincipal user);
    }

    public class GetUserRolesQuery : IGetUserRolesQuery
    {
        private readonly IKeycloakService _keycloakService;

        public GetUserRolesQuery(IKeycloakService keycloakService)
        {
            _keycloakService = keycloakService;
        }

        public async Task<ICollection<RoleRepresentation>> Handle(Guid userId, ClaimsPrincipal user)
        {
            try
            {
                var keycloakUser = await _keycloakService.GetUserByIdAsync(userId.ToString());
                if (keycloakUser == null)
                {
                    throw new InvalidOperationException($"User with ID '{userId}' not found in Keycloak.");
                }

                return await _keycloakService.GetUserRolesAsync(userId.ToString());
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to retrieve user roles: {ex.Message}", ex);
            }
        }
    }
}