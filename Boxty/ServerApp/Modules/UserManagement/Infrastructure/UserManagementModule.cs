using Boxty.ServerBase.Database;
using Boxty.ServerBase.Modules;
using Boxty.ServerBase.Queries.ModuleQueries;
using Boxty.ServerBase.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Boxty.ServerApp.Modules.UserManagement.Infrastructure.AuthorizationHandlers;
using Boxty.ServerApp.Modules.UserManagement.Infrastructure.Commands;
using Boxty.ServerApp.Modules.UserManagement.Infrastructure.Database;
using Boxty.ServerApp.Modules.UserManagement.Infrastructure.Queries;
using Boxty.ServerApp.Modules.UserManagement.Services;
using Boxty.ServerApp.Modules.Shared.Contracts;

namespace Boxty.ServerApp.Modules.UserManagement.Infrastructure
{
    public class UserManagementModule : IModule
    {
        public IServiceCollection RegisterModuleServices(IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            services.AddDbContext<IDbContext<UserManagementDbContext>, UserManagementDbContext>(options =>
                options.UseNpgsql(connectionString));

            services.AddScoped<IAuthorizationHandler, UserManagementResourceAccessAuthorizationHandler>();
            services.AddScoped<IAuthorizationHandler, UserManagementCreateEntityAuthorizationHandler>();
            services.AddScoped<IKeycloakService, KeycloakService>();
            services.AddScoped<IAuthorizationModelChangedNotifier, AuthorizationModelChangedNotifier>();

            services.AddScoped(typeof(ICreateSubjectCommand<,,>), typeof(CreateSubjectCommand<,,>));
            services.AddScoped(typeof(IUpdateSubjectCommand<,,>), typeof(UpdateSubjectCommand<,,>));
            services.AddScoped(typeof(ICreateTenantCommand<,,>), typeof(CreateTenantCommand<,,>));
            services.AddScoped(typeof(IResetPasswordCommand<,,>), typeof(ResetPasswordCommand<,,>));
            services.AddScoped(typeof(IDeleteTenantCommand<,,>), typeof(DeleteTenantCommand<,,>));
            services.AddScoped(typeof(IDeleteSubjectCommand<,,>), typeof(DeleteSubjectCommand<,,>));
            services.AddScoped<IAddUserRoleCommand, AddUserRoleCommand>();
            services.AddScoped<IRemoveUserRoleCommand, RemoveUserRoleCommand>();
            services.AddScoped<IUpdateUserRolesCommand, UpdateUserRolesCommand>();
            services.AddScoped<ICreateRoleCommand, CreateRoleCommand>();
            services.AddScoped<IDeleteRoleCommand, DeleteRoleCommand>();
            services.AddScoped<IGetUserRolesQuery, GetUserRolesQuery>();
            services.AddScoped<IGetAllRolesQuery, GetAllRolesQuery>();

            services.Replace(ServiceDescriptor.Scoped<IGetAllRolesWithPermissionsQuery, GetAllRolesWithPermissionsQuery>());

            return services;
        }
        public WebApplication ConfigureModuleServices(WebApplication app, bool isDevelopment)
        {
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<UserManagementDbContext>();
                dbContext.Database.Migrate();
                UserManagementSeedData.Seed(dbContext);
            }

            return app;
        }
    }
}
