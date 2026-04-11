using System.Security.Claims;
using Boxty.ServerApp.Modules.UserManagement.Services;
using FS.Keycloak.RestApiClient.Model;

namespace Boxty.ServerApp.Modules.UserManagement.Infrastructure.Queries
{
    public interface IGetAllRolesQuery
    {
        Task<ICollection<RoleRepresentation>> Handle(ClaimsPrincipal user);
    }

    public class GetAllRolesQuery : IGetAllRolesQuery
    {
        private readonly IKeycloakService _keycloakService;

        public GetAllRolesQuery(IKeycloakService keycloakService)
        {
            _keycloakService = keycloakService;
        }

        public async Task<ICollection<RoleRepresentation>> Handle(ClaimsPrincipal user)
        {
            try
            {
                return await _keycloakService.GetAllRolesAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to retrieve all roles: {ex.Message}", ex);
            }
        }
    }
}