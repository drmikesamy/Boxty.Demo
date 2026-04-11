using System.Security.Claims;
using Boxty.SharedBase.Interfaces;
using FluentValidation.Results;

namespace Boxty.ServerApp.Modules.UserManagement.Infrastructure.Commands
{
    internal static class SubjectAuthorizationHelper
    {
        public static List<ValidationFailure> ValidateRoleAssignment(
            IUserClaimsReader userClaimsReader,
            ClaimsPrincipal user,
            string? requestedRoleName,
            Guid tenantId,
            string unauthorizedMessage,
            string tenantScopeMessage)
        {
            var validationErrors = new List<ValidationFailure>();

            if (user?.Identity?.IsAuthenticated != true)
            {
                validationErrors.Add(new ValidationFailure("Authorization", "User must be authenticated to perform this action."));
                return validationErrors;
            }

            var userRoles = userClaimsReader.GetRoles(user);
            if (userRoles == null || !userRoles.Any())
            {
                validationErrors.Add(new ValidationFailure("Authorization", "User has no assigned roles."));
                return validationErrors;
            }

            if (!KeycloakAdminCommandHelper.CanManageSubjects(userRoles))
            {
                validationErrors.Add(new ValidationFailure("Authorization", unauthorizedMessage));
                return validationErrors;
            }

            var userMaxAuthority = KeycloakAdminCommandHelper.GetUserMaxAuthority(userRoles);

            var normalizedRequestedRole = KeycloakAdminCommandHelper.NormalizeRoleName(requestedRoleName ?? "subject");
            if (!KeycloakAdminCommandHelper.TryGetRoleAuthority(normalizedRequestedRole, out var requestedRoleAuthority))
            {
                if (!KeycloakAdminCommandHelper.CanAssignUnknownRole(userRoles))
                {
                    validationErrors.Add(new ValidationFailure("RoleName", $"Only administrators can assign unknown or custom roles like '{requestedRoleName}'."));
                }

                return validationErrors;
            }

            if (requestedRoleAuthority > userMaxAuthority)
            {
                var userRoleNames = KeycloakAdminCommandHelper.GetKnownUserRoleNames(userRoles);
                validationErrors.Add(new ValidationFailure("RoleName", $"User with role(s) '{userRoleNames}' cannot assign role '{requestedRoleName}'. Users can only assign roles at their authority level or below."));
            }

            var isTenantAdmin = KeycloakAdminCommandHelper.HasRole(userRoles, "tenantadministrator");
            var isTenantLimitedAdmin = KeycloakAdminCommandHelper.HasRole(userRoles, "tenantlimitedadministrator");
            var isFullAdmin = KeycloakAdminCommandHelper.HasRole(userRoles, "administrator");

            if ((isTenantAdmin || isTenantLimitedAdmin) && !isFullAdmin)
            {
                if (string.Equals(normalizedRequestedRole, "administrator", StringComparison.OrdinalIgnoreCase))
                {
                    var userType = isTenantAdmin ? "Tenant administrators" : "Tenant limited administrators";
                    validationErrors.Add(new ValidationFailure("RoleName", $"{userType} cannot assign administrator roles."));
                }

                if (isTenantLimitedAdmin && string.Equals(normalizedRequestedRole, "tenantadministrator", StringComparison.OrdinalIgnoreCase))
                {
                    validationErrors.Add(new ValidationFailure("RoleName", "Tenant limited administrators cannot assign tenant administrator roles."));
                }

                var userTenantId = userClaimsReader.GetOrganizationId(user);
                if (!string.IsNullOrEmpty(userTenantId) && Guid.TryParse(userTenantId, out var userTenant) && tenantId != userTenant)
                {
                    var userType = isTenantAdmin ? "Tenant administrators" : "Tenant limited administrators";
                    validationErrors.Add(new ValidationFailure("TenantId", $"{userType} can only {tenantScopeMessage} within their own tenant."));
                }
            }

            return validationErrors;
        }

        public static void EnsureCanResetPassword(IUserClaimsReader userClaimsReader, ClaimsPrincipal user, string? targetRoleName)
        {
            if (user?.Identity?.IsAuthenticated != true)
            {
                throw new UnauthorizedAccessException("User must be authenticated to reset passwords.");
            }

            var userRoles = userClaimsReader.GetRoles(user);
            if (userRoles == null || !userRoles.Any())
            {
                throw new UnauthorizedAccessException("User has no assigned roles.");
            }

            if (!KeycloakAdminCommandHelper.CanManageSubjects(userRoles))
            {
                throw new UnauthorizedAccessException("User does not have permission to reset passwords. Requires 'administrator', 'tenantadministrator', or 'tenantlimitedadministrator' role.");
            }

            var userMaxAuthority = KeycloakAdminCommandHelper.GetUserMaxAuthority(userRoles);
            var normalizedTargetRole = KeycloakAdminCommandHelper.NormalizeRoleName(targetRoleName ?? "subject");
            var targetAuthority = KeycloakAdminCommandHelper.GetRoleAuthorityOrDefault(normalizedTargetRole);

            if (targetAuthority > userMaxAuthority)
            {
                var userRoleNames = KeycloakAdminCommandHelper.GetKnownUserRoleNames(userRoles);
                throw new UnauthorizedAccessException($"User with role(s) '{userRoleNames}' does not have permission to reset passwords for users with role '{targetRoleName}'. Users can only reset passwords for roles at their authority level or below.");
            }

            var isTenantLimitedAdmin = KeycloakAdminCommandHelper.HasRole(userRoles, "tenantlimitedadministrator");
            var isFullAdmin = KeycloakAdminCommandHelper.HasRole(userRoles, "administrator");

            if (isTenantLimitedAdmin && !isFullAdmin && !string.Equals(normalizedTargetRole, "subject", StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException($"Tenant limited administrators can only reset passwords for subjects, not for users with role '{targetRoleName}'.");
            }
        }
    }
}