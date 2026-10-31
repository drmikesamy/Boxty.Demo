using Boxty.ServerBase.Database;
using Boxty.ServerBase.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Boxty.ServerApp.Modules.DBExplorer.Infrastructure.Database;

namespace Boxty.ServerApp.Modules.DBExplorer.Infrastructure
{
    public class DBExplorerModule : IModule
    {
        public IServiceCollection RegisterModuleServices(IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            services.AddDbContext<IDbContext<DBExplorerDbContext>, DBExplorerDbContext>(options =>
                options.UseNpgsql(connectionString));

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
