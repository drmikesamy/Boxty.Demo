using Boxty.ServerBase.Auth.Requirements;
using Boxty.ServerApp.Modules.UserManagement.Infrastructure.Commands;
using Boxty.SharedBase.DTOs;
using Boxty.SharedBase.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace Boxty.ServerApp.Modules.UserManagement.Infrastructure.AuthorizationHandlers
{
    public class UserManagementCreateEntityAuthorizationHandler : AuthorizationHandler<CreateEntityRequirement, IAuditDto>
    {
        private readonly IUserClaimsReader _userClaimsReader;

        public UserManagementCreateEntityAuthorizationHandler(IUserClaimsReader userClaimsReader)
        {
            _userClaimsReader = userClaimsReader;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, CreateEntityRequirement requirement, IAuditDto resource)
        {
            if (resource is not ISubject subject)
            {
                return Task.CompletedTask;
            }

            var failures = SubjectAuthorizationHelper.ValidateRoleAssignment(
                _userClaimsReader,
                context.User,
                subject.RoleName,
                resource.TenantId,
                "You are not authorised to create subjects.",
                "create subjects");

            if (failures.Count == 0)
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            foreach (var failure in failures)
            {
                context.Fail(new AuthorizationFailureReason(this, $"{failure.PropertyName}|{failure.ErrorMessage}"));
            }

            return Task.CompletedTask;
        }
    }
}