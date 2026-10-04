using Boxty.ServerBase.Auth.Providers;
using Boxty.ServerBase.Database;
using Boxty.ServerBase.Modules;
using Boxty.ServerBase.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Boxty.ServerApp.Modules.DBExplorer.Entities;
using Boxty.ServerApp.Modules.DBExplorer.Infrastructure.Database;
using Boxty.ServerApp.Modules.Shared.Contracts;

namespace Boxty.ServerApp.Modules.DBExplorer.Infrastructure
{
    public class DBExplorerModule : IModule
    {
        public IServiceCollection RegisterModuleServices(IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            services.AddDbContext<IDbContext<DBExplorerDbContext>, DBExplorerDbContext>(options =>
                            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            // Add your service registrations here
            // Example: services.AddScoped<IYourQuery, YourQuery>();
            // Example: services.AddScoped<IAuthorizationHandler, YourAuthorizationHandler>();

            return services;
        }
        
        public WebApplication ConfigureModuleServices(WebApplication app, bool isDevelopment)
        {
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<DBExplorerDbContext>();
                dbContext.Database.Migrate();
            }
            return app;
        }
    }
}
