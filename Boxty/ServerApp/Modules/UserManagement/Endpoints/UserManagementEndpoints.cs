using Boxty.ServerBase.Endpoints;
using Boxty.ServerApp.Modules.UserManagement.Entities;
using Boxty.ServerApp.Modules.UserManagement.Infrastructure.Commands;
using Boxty.ServerApp.Modules.UserManagement.Infrastructure.Database;
using Boxty.ServerApp.Modules.UserManagement.Infrastructure.Queries;
using Boxty.ServerApp.Modules.Shared.Contracts;
using Boxty.SharedBase.DTOs.Auth;
using Boxty.SharedApp.DTOs.UserManagement;
using Microsoft.Extensions.DependencyInjection;

namespace Boxty.ServerApp.Modules.UserManagement.Endpoints
{
    public abstract class RolePermissionCacheAwareEndpoints<T, TDto, TContext> : BaseEndpoints<T, TDto, TContext>
        where T : class, Boxty.ServerBase.Entities.IEntity
        where TDto : Boxty.SharedBase.DTOs.IDto
        where TContext : Boxty.ServerBase.Database.IDbContext<TContext>
    {
        protected override Task OnAfterCreate(Guid createdId, System.Security.Claims.ClaimsPrincipal user, TDto originalDto, IServiceProvider serviceProvider)
        {
            return RefreshRolePermissionCacheAsync(serviceProvider);
        }

        protected override Task OnAfterUpdate(Guid updatedId, System.Security.Claims.ClaimsPrincipal user, TDto originalDto, IServiceProvider serviceProvider)
        {
            return RefreshRolePermissionCacheAsync(serviceProvider);
        }

        protected override Task OnAfterDelete(Guid deletedId, System.Security.Claims.ClaimsPrincipal user, IServiceProvider serviceProvider)
        {
            return RefreshRolePermissionCacheAsync(serviceProvider);
        }

        private static Task RefreshRolePermissionCacheAsync(IServiceProvider serviceProvider)
        {
            var authorizationModelChangedNotifier = serviceProvider.GetRequiredService<IAuthorizationModelChangedNotifier>();
            return authorizationModelChangedNotifier.NotifyChangedAsync();
        }
    }

    public class SubjectSecurityEndpoints : KeycloakSubjectEndpoints<Subject, SubjectDto, UserManagementDbContext>, IEndpoints { }
    public class TenantSecurityEndpoints : KeycloakTenantEndpoints<Tenant, TenantDto, UserManagementDbContext>, IEndpoints { }
    public class RoleSecurityEndpoints : KeycloakRoleEndpoints<Role, UserManagementDbContext>, IEndpoints { }
    public class TenantDocumentEndpoints : DocumentEndpoints<TenantDocument, TenantDocumentDto, UserManagementDbContext>, IEndpoints { }
    public class SubjectDocumentEndpoints : DocumentEndpoints<SubjectDocument, SubjectDocumentDto, UserManagementDbContext>, IEndpoints { }
    public class TenantNoteEndpoints : BaseEndpoints<TenantNote, TenantNoteDto, UserManagementDbContext>, IEndpoints { }
    public class SubjectNoteEndpoints : BaseEndpoints<SubjectNote, SubjectNoteDto, UserManagementDbContext>, IEndpoints { }
    public class PermissionEndpoints : RolePermissionCacheAwareEndpoints<Permission, PermissionDto, UserManagementDbContext>, IEndpoints { }
}
