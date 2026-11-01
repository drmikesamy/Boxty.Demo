using Boxty.ServerBase.Database;
using Boxty.ServerBase.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Boxty.ServerApp.Modules.DemoModule.Infrastructure.Database;

namespace Boxty.ServerApp.Modules.DemoModule.Infrastructure
{
    public class DemoModuleModule : IModule
    {
        public IServiceCollection RegisterModuleServices(IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            services.AddDbContext<DemoModuleDbContext>(options =>
                options.UseNpgsql(connectionString));
            services.AddScoped<IDbContext<DemoModuleDbContext>>(sp =>
                sp.GetRequiredService<DemoModuleDbContext>());

            return services;
        }

        public WebApplication ConfigureModuleServices(WebApplication app, bool isDevelopment)
        {
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<DemoModuleDbContext>();
                dbContext.Database.Migrate();
            }

            return app;
        }
    }
}
