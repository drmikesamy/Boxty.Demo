using System.Reflection;
using System.Security.Claims;
using Boxty.ServerBase.Commands;
using Boxty.ServerApp.Modules.UserManagement.Infrastructure.Commands;
using Boxty.SharedBase.DTOs.Auth;
using Boxty.ServerBase.Auth.Constants;
using Boxty.ServerBase.Database;
using Boxty.ServerBase.Endpoints;
using Boxty.ServerBase.Entities;
using Boxty.SharedBase.DTOs;
using Boxty.SharedBase.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Boxty.ServerApp.Modules.Shared.Contracts;

namespace Boxty.ServerApp.Modules.UserManagement.Endpoints
{
    public abstract class KeycloakSubjectEndpoints<T, TDto, TContext> : BaseEndpoints<T, TDto, TContext>
        where T : class, IEntity, ISubjectEntity
        where TDto : IDto, IAuditDto, ISubject
        where TContext : IDbContext<TContext>
    {
        public override IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
        {
            endpoints = base.MapEndpoints(endpoints);

            var endpointName = typeof(T).Name;
            var group = endpoints.MapGroup($"/api/{endpointName}");

            group.MapPut("/resetpassword/{id:guid}", async (
                [FromServices] IResetPasswordCommand<T, TDto, TContext> resetPasswordCommand,
                ClaimsPrincipal user,
                Guid id
            ) => await ResetPassword(resetPasswordCommand, user, id))
            .RequireAuthorization($"Permission:Create{typeof(T).Name}");

            return endpoints;
        }

        protected override void MapCreateEndpoint(RouteGroupBuilder group)
        {
            var createPermission = PermissionHelper.GeneratePermission<T>(PermissionOperations.Create);

            group.MapPost("/Create", (
                [FromServices] ICreateSubjectCommand<T, TDto, TContext> createSubjectCommand,
                ClaimsPrincipal user,
                TDto dto
            ) => Create(createSubjectCommand, user, dto))
            .RequireAuthorization($"Permission:{createPermission}");
        }

        protected override void MapDeleteEndpoint(RouteGroupBuilder group)
        {
            var deletePermission = PermissionHelper.GeneratePermission<T>(PermissionOperations.Delete);

            group.MapDelete("/Delete/{id}", (
                [FromServices] IDeleteSubjectCommand<T, TDto, TContext> deleteCommand,
                ClaimsPrincipal user,
                Guid id
            ) => Delete(deleteCommand, user, id))
            .RequireAuthorization($"Permission:{deletePermission}");
        }

        protected override void MapUpdateEndpoint(RouteGroupBuilder group)
        {
            var updatePermission = PermissionHelper.GeneratePermission<T>(PermissionOperations.Update);

            group.MapPut("/Update", (
                [FromServices] IUpdateSubjectCommand<T, TDto, TContext> updateSubjectCommand,
                ClaimsPrincipal user,
                TDto dto
            ) => Update(updateSubjectCommand, user, dto))
            .RequireAuthorization($"Permission:{updatePermission}");
        }

        protected async Task<IResult> Create(ICreateSubjectCommand<T, TDto, TContext> createSubjectCommand, ClaimsPrincipal user, TDto dto)
        {
            return await ExecuteWithValidation(async () =>
            {
                var result = await createSubjectCommand.Handle(dto, user);
                return Results.Ok(result);
            }, $"An error occurred while creating the {typeof(T).Name}.");
        }

        protected async Task<IResult> Delete(IDeleteSubjectCommand<T, TDto, TContext> deleteCommand, ClaimsPrincipal user, Guid id)
        {
            return await Execute(async () =>
            {
                var result = await deleteCommand.Handle(id, user);
                return Results.Ok(result);
            }, $"An error occurred while deleting the {typeof(T).Name}.");
        }

        protected async Task<IResult> Update(IUpdateSubjectCommand<T, TDto, TContext> updateSubjectCommand, ClaimsPrincipal user, TDto dto)
        {
            return await ExecuteWithValidation(async () =>
            {
                var result = await updateSubjectCommand.Handle(dto, user);
                return Results.Ok(result);
            }, $"An error occurred while updating the {typeof(T).Name}.");
        }

        protected async Task<IResult> ResetPassword(IResetPasswordCommand<T, TDto, TContext> resetPasswordCommand, ClaimsPrincipal user, Guid id)
        {
            try
            {
                var result = await resetPasswordCommand.Handle(id, user);
                return Results.Ok(result);
            }
            catch (ValidationException ex)
            {
                var errors = ex.Errors.Select(e => new { Field = e.PropertyName, Message = e.ErrorMessage });
                return Results.BadRequest(new { Message = "Validation failed", Errors = errors });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { Message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Forbid();
            }
            catch (ArgumentNullException ex)
            {
                return Results.BadRequest(new { Message = $"Required field missing: {ex.ParamName}" });
            }
            catch (Exception)
            {
                return Results.Problem("An unexpected error occurred while resetting the password. Please try again.");
            }
        }
    }

    public abstract class KeycloakTenantEndpoints<T, TDto, TContext> : BaseEndpoints<T, TDto, TContext>
        where T : class, IEntity, ITenantEntity
        where TDto : IDto, ITenant
        where TContext : IDbContext<TContext>
    {
        protected override void MapCreateEndpoint(RouteGroupBuilder group)
        {
            var createPermission = PermissionHelper.GeneratePermission<T>(PermissionOperations.Create);

            group.MapPost("/Create", (
                [FromServices] ICreateTenantCommand<T, TDto, TContext> createTenantCommand,
                ClaimsPrincipal user,
                TDto dto
            ) => Create(createTenantCommand, user, dto))
            .RequireAuthorization($"Permission:{createPermission}");
        }

        protected override void MapDeleteEndpoint(RouteGroupBuilder group)
        {
            var deletePermission = PermissionHelper.GeneratePermission<T>(PermissionOperations.Delete);

            group.MapDelete("/Delete/{id}", (
                [FromServices] IDeleteTenantCommand<T, TDto, TContext> deleteCommand,
                ClaimsPrincipal user,
                Guid id
            ) => Delete(deleteCommand, user, id))
            .RequireAuthorization($"Permission:{deletePermission}");
        }

        protected async Task<IResult> Create(ICreateTenantCommand<T, TDto, TContext> createTenantCommand, ClaimsPrincipal user, TDto dto)
        {
            return await ExecuteWithValidation(async () =>
            {
                var result = await createTenantCommand.Handle(dto, user);
                return Results.Ok(result);
            }, $"An error occurred while creating the {typeof(T).Name}.");
        }

        protected async Task<IResult> Delete(IDeleteTenantCommand<T, TDto, TContext> deleteCommand, ClaimsPrincipal user, Guid id)
        {
            return await Execute(async () =>
            {
                var result = await deleteCommand.Handle(id, user);
                return Results.Ok(result);
            }, $"An error occurred while deleting the {typeof(T).Name}.");
        }
    }

    public abstract class KeycloakRoleEndpoints<T, TContext> : BaseEndpoints<T, RoleDto, TContext>
        where T : class, IEntity
        where TContext : IDbContext<TContext>
    {
        protected override void MapCreateEndpoint(RouteGroupBuilder group)
        {
            var createPermission = PermissionHelper.GeneratePermission<T>(PermissionOperations.Create);

            group.MapPost("/Create", (
                [FromServices] ICreateCommand<T, RoleDto, TContext> createCommand,
                [FromServices] IServiceProvider serviceProvider,
                ClaimsPrincipal user,
                RoleDto dto
            ) => Create(createCommand, user, dto, serviceProvider))
            .RequireAuthorization($"Permission:{createPermission}");
        }

        protected override void MapDeleteEndpoint(RouteGroupBuilder group)
        {
            var deletePermission = PermissionHelper.GeneratePermission<T>(PermissionOperations.Delete);

            group.MapDelete("/Delete/{id}", (
                [FromServices] IDeleteCommand<T, TContext> deleteCommand,
                [FromServices] IServiceProvider serviceProvider,
                ClaimsPrincipal user,
                Guid id
            ) => Delete(deleteCommand, user, id, serviceProvider))
            .RequireAuthorization($"Permission:{deletePermission}");
        }

        protected override async Task<IResult> Create(ICreateCommand<T, RoleDto, TContext> createCommand, ClaimsPrincipal user, RoleDto dto, IServiceProvider serviceProvider)
        {
            return await ExecuteWithValidation(async () =>
            {
                var createRoleCommand = serviceProvider.GetRequiredService<ICreateRoleCommand>();
                var deleteRoleCommand = serviceProvider.GetRequiredService<IDeleteRoleCommand>();

                await createRoleCommand.Handle(dto.Name, null, user);

                try
                {
                    var result = await createCommand.Handle(dto, user);
                    return Results.Ok(result);
                }
                catch
                {
                    await TryDeleteRoleAsync(deleteRoleCommand, dto.Name, user);
                    throw;
                }
            }, $"An error occurred while creating the {typeof(T).Name}.");
        }

        protected override Task OnAfterCreate(Guid createdId, ClaimsPrincipal user, RoleDto originalDto, IServiceProvider serviceProvider)
        {
            return RefreshRolePermissionCacheAsync(serviceProvider);
        }

        protected override Task OnAfterUpdate(Guid updatedId, ClaimsPrincipal user, RoleDto originalDto, IServiceProvider serviceProvider)
        {
            return RefreshRolePermissionCacheAsync(serviceProvider);
        }

        protected override Task OnAfterDelete(Guid deletedId, ClaimsPrincipal user, IServiceProvider serviceProvider)
        {
            return RefreshRolePermissionCacheAsync(serviceProvider);
        }

        protected new async Task<IResult> Delete(IDeleteCommand<T, TContext> deleteCommand, ClaimsPrincipal user, Guid id, IServiceProvider serviceProvider)
        {
            return await Execute(async () =>
            {
                var dbContext = serviceProvider.GetRequiredService<IDbContext<TContext>>();
                var createRoleCommand = serviceProvider.GetRequiredService<ICreateRoleCommand>();
                var deleteRoleCommand = serviceProvider.GetRequiredService<IDeleteRoleCommand>();

                var entity = await dbContext.Set<T>().AsNoTracking().SingleOrDefaultAsync(e => e.Id == id);
                if (entity == null)
                {
                    return Results.Ok(false);
                }

                var roleName = GetRoleName(entity);
                await deleteRoleCommand.Handle(roleName, user);

                try
                {
                    var result = await deleteCommand.Handle(id, user);
                    return Results.Ok(result);
                }
                catch
                {
                    await TryCreateRoleAsync(createRoleCommand, roleName, user);
                    throw;
                }
            }, $"An error occurred while deleting the {typeof(T).Name}.");
        }

        private static string GetRoleName(T entity)
        {
            var nameProperty = typeof(T).GetProperty("Name", BindingFlags.Public | BindingFlags.Instance);
            if (nameProperty?.PropertyType != typeof(string))
            {
                throw new InvalidOperationException($"{typeof(T).Name} must expose a public string Name property to use Keycloak role endpoints.");
            }

            var roleName = nameProperty.GetValue(entity) as string;
            if (string.IsNullOrWhiteSpace(roleName))
            {
                throw new InvalidOperationException($"{typeof(T).Name} must provide a non-empty role name to synchronize with Keycloak.");
            }

            return roleName;
        }

        private static async Task TryDeleteRoleAsync(IDeleteRoleCommand deleteRoleCommand, string roleName, ClaimsPrincipal user)
        {
            try
            {
                await deleteRoleCommand.Handle(roleName, user);
            }
            catch
            {
            }
        }

        private static async Task TryCreateRoleAsync(ICreateRoleCommand createRoleCommand, string roleName, ClaimsPrincipal user)
        {
            try
            {
                await createRoleCommand.Handle(roleName, null, user);
            }
            catch
            {
            }
        }

        private static Task RefreshRolePermissionCacheAsync(IServiceProvider serviceProvider)
        {
            var authorizationModelChangedNotifier = serviceProvider.GetRequiredService<IAuthorizationModelChangedNotifier>();
            return authorizationModelChangedNotifier.NotifyChangedAsync();
        }
    }
}