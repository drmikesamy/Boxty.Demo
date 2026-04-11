using Boxty.ServerBase.Auth.Requirements;
using Boxty.ServerBase.Entities;
using Boxty.ServerApp.Modules.UserManagement.Entities;
using Boxty.ServerApp.Modules.UserManagement.Infrastructure.Commands;
using Boxty.SharedBase.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace Boxty.ServerApp.Modules.UserManagement.Infrastructure.AuthorizationHandlers
{
    public class UserManagementResourceAccessAuthorizationHandler :
        AuthorizationHandler<ResourceAccessRequirement, IEntity>
    {
        private readonly IUserClaimsReader _userClaimsReader;

        public UserManagementResourceAccessAuthorizationHandler(IUserClaimsReader userClaimsReader)
        {
            _userClaimsReader = userClaimsReader;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            ResourceAccessRequirement requirement,
            IEntity resource)
        {
            if (resource is not Tenant and
                not Subject)
            {
                return Task.CompletedTask;
            }

            if (context.User.IsInRole("administrator"))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            var userRoles = _userClaimsReader.GetRoles(context.User);
            if (userRoles.Count == 0)
            {
                return Task.CompletedTask;
            }

            var userTenantId = _userClaimsReader.GetOrganizationId(context.User);
            var hasTenantScopedAccess = !string.IsNullOrEmpty(userTenantId)
                && Guid.TryParse(userTenantId, out var tenantId)
                && (resource.TenantId == tenantId || resource.Id == tenantId);

            if (resource is Tenant && hasTenantScopedAccess &&
                (KeycloakAdminCommandHelper.HasRole(userRoles, "tenantadministrator")
                || KeycloakAdminCommandHelper.HasRole(userRoles, "tenantlimitedadministrator")))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            if (resource is Subject subject)
            {
                if (!KeycloakAdminCommandHelper.CanManageSubjects(userRoles) || !hasTenantScopedAccess)
                {
                    return Task.CompletedTask;
                }

                var userMaxAuthority = KeycloakAdminCommandHelper.GetUserMaxAuthority(userRoles);
                var subjectRole = KeycloakAdminCommandHelper.NormalizeRoleName(subject.RoleName ?? "subject");
                var subjectAuthority = KeycloakAdminCommandHelper.GetRoleAuthorityOrDefault(subjectRole);

                if (subjectAuthority <= userMaxAuthority)
                {
                    context.Succeed(requirement);
                }
            }

            return Task.CompletedTask;
        }
    }
}
