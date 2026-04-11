using Boxty.ServerBase.Config;
using FS.Keycloak.RestApiClient.Api;
using FS.Keycloak.RestApiClient.Authentication.ClientFactory;
using FS.Keycloak.RestApiClient.Authentication.Flow;
using FS.Keycloak.RestApiClient.ClientFactory;
using FS.Keycloak.RestApiClient.Model;
using Microsoft.Extensions.Options;
using System.Linq;

namespace Boxty.ServerApp.Modules.UserManagement.Services
{
    public interface IKeycloakService
    {
        // Basic operations
        string GetRealmName();

        // User operations
        Task<UserRepresentation?> GetUserByIdAsync(string userId);
        Task<ICollection<UserRepresentation>> GetUsersAsync(string search, int max);
        Task PostUsersAsync(UserRepresentation userRepresentation);
        Task DeleteUserAsync(string userId);
        Task ResetUserPasswordAsync(string userId, CredentialRepresentation credential);

        // Organization operations
        Task<ICollection<OrganizationRepresentation>> GetOrganizationsAsync(string? name = null);
        Task<OrganizationRepresentation> GetOrganizationByIdAsync(string orgId);
        Task PostOrganizationAsync(OrganizationRepresentation organizationRepresentation);
        Task PostOrganizationMemberAsync(string orgId, string userId);
        Task DeleteOrganizationAsync(string orgId);

        // Role operations
        Task<RoleRepresentation> GetRoleByNameAsync(string roleName);
        Task<ICollection<RoleRepresentation>> GetAllRolesAsync();
        Task<ICollection<RoleRepresentation>> GetUserRolesAsync(string userId);
        Task PostUserRoleMappingAsync(string userId, ICollection<RoleRepresentation> roles);
        Task DeleteUserRoleMappingAsync(string userId, ICollection<RoleRepresentation> roles);
        Task CreateRoleAsync(RoleRepresentation role);
        Task DeleteRoleAsync(string roleName);
        Task UpdateRoleAsync(string roleName, RoleRepresentation role);
        Task UpdateUserAsync(string userId, UserRepresentation userRepresentation);
    }

    public class KeycloakService : IKeycloakService
    {
        private readonly AppOptions _options;

        public KeycloakService(IOptions<AppOptions> options)
        {
            _options = options.Value;
        }

        public string GetRealmName()
        {
            return _options.KeycloakAdminClient.Realm;
        }

        // User operations
        public async Task<ICollection<UserRepresentation>> GetUsersAsync(string search, int max)
        {
            using var usersApi = CreateUsersApi();
            return await usersApi.GetUsersAsync(GetRealmName(), true, null, null, null, true, null, null, null, null, null, max, null, search);
        }
        public async Task<UserRepresentation?> GetUserByIdAsync(string userId)
        {
            using var usersApi = CreateUsersApi();
            return await usersApi.GetUsersByUserIdAsync(GetRealmName(), userId);
        }

        public async Task PostUsersAsync(UserRepresentation userRepresentation)
        {
            using var usersApi = CreateUsersApi();
            await usersApi.PostUsersAsync(GetRealmName(), userRepresentation);
        }

        public async Task DeleteUserAsync(string userId)
        {
            using var usersApi = CreateUsersApi();
            await usersApi.DeleteUsersByUserIdAsync(GetRealmName(), userId);
        }

        public async Task ResetUserPasswordAsync(string userId, CredentialRepresentation credential)
        {
            using var usersApi = CreateUsersApi();
            await usersApi.PutUsersResetPasswordByUserIdAsync(GetRealmName(), userId, credential);
        }

        // Organization operations
        public async Task<ICollection<OrganizationRepresentation>> GetOrganizationsAsync(string? name = null)
        {
            using var organizationsApi = CreateOrganizationsApi();
            return await organizationsApi.GetOrganizationsAsync(GetRealmName(), true, true, null, 100, null, name);
        }

        public async Task<OrganizationRepresentation> GetOrganizationByIdAsync(string orgId)
        {
            using var organizationsApi = CreateOrganizationsApi();
            return await organizationsApi.GetOrganizationsByOrgIdAsync(GetRealmName(), orgId);
        }

        public async Task PostOrganizationAsync(OrganizationRepresentation organizationRepresentation)
        {
            using var organizationsApi = CreateOrganizationsApi();
            await organizationsApi.PostOrganizationsAsync(GetRealmName(), organizationRepresentation);
        }

        public async Task PostOrganizationMemberAsync(string orgId, string userId)
        {
            using var organizationsApi = CreateOrganizationsApi();
            await organizationsApi.PostOrganizationsMembersByOrgIdAsync(GetRealmName(), orgId, userId);
        }

        public async Task DeleteOrganizationAsync(string orgId)
        {
            using var organizationsApi = CreateOrganizationsApi();
            await organizationsApi.DeleteOrganizationsByOrgIdAsync(GetRealmName(), orgId);
        }

        // Role operations
        public async Task<RoleRepresentation> GetRoleByNameAsync(string roleName)
        {
            using var rolesApi = CreateRolesApi();
            return await rolesApi.GetRolesByRoleNameAsync(GetRealmName(), roleName.ToLowerInvariant());
        }

        public async Task<ICollection<RoleRepresentation>> GetAllRolesAsync()
        {
            using var rolesApi = CreateRolesApi();
            return await rolesApi.GetRolesAsync(GetRealmName());
        }

        public async Task<ICollection<RoleRepresentation>> GetUserRolesAsync(string userId)
        {
            using var roleMappingApi = CreateRoleMapperApi();
            return await roleMappingApi.GetUsersRoleMappingsRealmByUserIdAsync(GetRealmName(), userId);
        }

        public async Task PostUserRoleMappingAsync(string userId, ICollection<RoleRepresentation> roles)
        {
            using var roleMappingApi = CreateRoleMapperApi();
            await roleMappingApi.PostUsersRoleMappingsRealmByUserIdAsync(GetRealmName(), userId, roles.ToList());
        }

        public async Task DeleteUserRoleMappingAsync(string userId, ICollection<RoleRepresentation> roles)
        {
            using var roleMappingApi = CreateRoleMapperApi();
            await roleMappingApi.DeleteUsersRoleMappingsRealmByUserIdAsync(GetRealmName(), userId, roles.ToList());
        }

        public async Task CreateRoleAsync(RoleRepresentation role)
        {
            using var rolesApi = CreateRolesApi();
            await rolesApi.PostRolesAsync(GetRealmName(), role);
        }

        public async Task DeleteRoleAsync(string roleName)
        {
            using var rolesApi = CreateRolesApi();
            await rolesApi.DeleteRolesByRoleNameAsync(GetRealmName(), roleName.ToLowerInvariant());
        }

        public async Task UpdateRoleAsync(string roleName, RoleRepresentation role)
        {
            using var rolesApi = CreateRolesApi();
            await rolesApi.PutRolesByRoleNameAsync(GetRealmName(), roleName.ToLowerInvariant(), role);
        }

        public async Task UpdateUserAsync(string userId, UserRepresentation userRepresentation)
        {
            using var usersApi = CreateUsersApi();
            await usersApi.PutUsersByUserIdAsync(GetRealmName(), userId, userRepresentation);
        }

        // Private helper to create ClientCredentialsFlow
        private ClientCredentialsFlow CreateClientCredentialsFlow()
        {
            var kc = _options.KeycloakAdminClient;
            return new ClientCredentialsFlow
            {
                KeycloakUrl = kc.KeycloakUrl,
                Realm = kc.Realm,
                ClientId = kc.ClientId,
                ClientSecret = kc.ClientSecret
            };
        }

        // Private methods to create API instances
        private OrganizationsApi CreateOrganizationsApi()
        {
            var credentials = CreateClientCredentialsFlow();
            var httpClient = AuthenticationHttpClientFactory.Create(credentials);
            return ApiClientFactory.Create<OrganizationsApi>(httpClient);
        }

        private UsersApi CreateUsersApi()
        {
            var credentials = CreateClientCredentialsFlow();
            var httpClient = AuthenticationHttpClientFactory.Create(credentials);
            return ApiClientFactory.Create<UsersApi>(httpClient);
        }

        private RoleMapperApi CreateRoleMapperApi()
        {
            var credentials = CreateClientCredentialsFlow();
            var httpClient = AuthenticationHttpClientFactory.Create(credentials);
            return ApiClientFactory.Create<RoleMapperApi>(httpClient);
        }

        private RolesApi CreateRolesApi()
        {
            var credentials = CreateClientCredentialsFlow();
            var httpClient = AuthenticationHttpClientFactory.Create(credentials);
            return ApiClientFactory.Create<RolesApi>(httpClient);
        }
    }
}
