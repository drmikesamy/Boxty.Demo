using Boxty.ServerBase.Database;
using Boxty.ServerBase.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Boxty.ServerApp.Modules.BoxtyModuleTemplate.Infrastructure.Database;

namespace Boxty.ServerApp.Modules.BoxtyModuleTemplate.Infrastructure
{
    public class BoxtyModuleTemplateModule : IModule
    {
        public IServiceCollection RegisterModuleServices(IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            services.AddDbContext<BoxtyModuleTemplateDbContext>(options =>
                options.UseNpgsql(connectionString));
            services.AddScoped<IDbContext<BoxtyModuleTemplateDbContext>>(sp =>
                sp.GetRequiredService<BoxtyModuleTemplateDbContext>());

            return services;
        }

        public WebApplication ConfigureModuleServices(WebApplication app, bool isDevelopment)
        {
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<BoxtyModuleTemplateDbContext>();
                dbContext.Database.Migrate();
            }

            return app;
        }
    }
}
