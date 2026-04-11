using Boxty.ServerApp.Modules.UserManagement.Services;
using FS.Keycloak.RestApiClient.Model;
using System.Net;

namespace Boxty.ServerApp.Modules.UserManagement.Infrastructure.Commands
{
    public static class KeycloakAdminCommandHelper
    {
        private static readonly IReadOnlyDictionary<string, int> RoleHierarchy = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["subject"] = 1,
            ["tenantlimitedadministrator"] = 2,
            ["tenantadministrator"] = 3,
            ["administrator"] = 4
        };

        public static string NormalizeRoleName(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                throw new ArgumentNullException(nameof(roleName));
            }

            return roleName.Trim().ToLowerInvariant();
        }

        public static async Task EnsureUserExistsAsync(IKeycloakService keycloakService, Guid userId)
        {
            var keycloakUser = await keycloakService.GetUserByIdAsync(userId.ToString());
            if (keycloakUser == null)
            {
                throw new InvalidOperationException($"User with ID '{userId}' not found in Keycloak.");
            }
        }

        public static async Task<RoleRepresentation> GetRequiredRoleAsync(IKeycloakService keycloakService, string roleName)
        {
            var normalizedRoleName = NormalizeRoleName(roleName);
            var role = await keycloakService.GetRoleByNameAsync(normalizedRoleName);
            if (role == null)
            {
                throw new InvalidOperationException($"Role '{normalizedRoleName}' not found in Keycloak.");
            }

            return role;
        }

        public static async Task<List<RoleRepresentation>> GetRequiredRolesAsync(IKeycloakService keycloakService, IEnumerable<string> roleNames)
        {
            var roles = new List<RoleRepresentation>();

            foreach (var roleName in roleNames)
            {
                roles.Add(await GetRequiredRoleAsync(keycloakService, roleName));
            }

            return roles;
        }

        public static bool HasRole(IEnumerable<string> roles, string roleName)
        {
            return roles.Any(role => string.Equals(role, roleName, StringComparison.OrdinalIgnoreCase));
        }

        public static bool CanManageSubjects(IEnumerable<string> roles)
        {
            return HasRole(roles, "administrator")
                || HasRole(roles, "tenantadministrator")
                || HasRole(roles, "tenantlimitedadministrator");
        }

        public static bool CanAssignUnknownRole(IEnumerable<string> roles)
        {
            return HasRole(roles, "administrator");
        }

        public static int GetUserMaxAuthority(IEnumerable<string> roles)
        {
            return roles
                .Where(role => RoleHierarchy.ContainsKey(role))
                .Select(role => RoleHierarchy[role])
                .DefaultIfEmpty(0)
                .Max();
        }

        public static string GetKnownUserRoleNames(IEnumerable<string> roles)
        {
            return string.Join(", ", roles.Where(role => RoleHierarchy.ContainsKey(role)));
        }

        public static bool TryGetRoleAuthority(string? roleName, out int authority)
        {
            return RoleHierarchy.TryGetValue(roleName ?? string.Empty, out authority);
        }

        public static int GetRoleAuthorityOrDefault(string? roleName, int defaultAuthority = 1)
        {
            return TryGetRoleAuthority(roleName, out var authority) ? authority : defaultAuthority;
        }

        public static bool IsNotFound(Exception ex)
        {
            if (ex is null)
            {
                return false;
            }

            if (HasStatusCode(ex, 404))
            {
                return true;
            }

            if (!string.IsNullOrEmpty(ex.Message)
                && (ex.Message.Contains("404", StringComparison.OrdinalIgnoreCase)
                    || ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            if (ex is AggregateException aggregateException)
            {
                return aggregateException.InnerExceptions.Any(IsNotFound);
            }

            return ex.InnerException != null && IsNotFound(ex.InnerException);
        }

        private static bool HasStatusCode(Exception ex, int expectedStatusCode)
        {
            var exType = ex.GetType();
            var statusCodeProperty = exType.GetProperty("StatusCode") ?? exType.GetProperty("ErrorCode");
            if (statusCodeProperty == null)
            {
                return false;
            }

            var value = statusCodeProperty.GetValue(ex);
            if (value is int intStatusCode)
            {
                return intStatusCode == expectedStatusCode;
            }

            if (value is HttpStatusCode httpStatusCode)
            {
                return (int)httpStatusCode == expectedStatusCode;
            }

            if (value is string stringStatusCode && int.TryParse(stringStatusCode, out var parsedStatusCode))
            {
                return parsedStatusCode == expectedStatusCode;
            }

            return false;
        }
    }
}