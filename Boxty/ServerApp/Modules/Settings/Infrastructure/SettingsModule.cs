using Boxty.ServerBase.Database;
using Boxty.ServerBase.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Boxty.ServerApp.Modules.Settings.Infrastructure.Database;
using Boxty.ServerApp.Modules.Settings.Infrastructure.Commands;

namespace Boxty.ServerApp.Modules.Settings.Infrastructure
{
    public class SettingsModule : IModule
    {
        public IServiceCollection RegisterModuleServices(IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            services.AddDbContext<SettingsDbContext>(options =>
                options.UseNpgsql(connectionString));
            
            services.AddScoped<IDbContext<SettingsDbContext>>(sp => sp.GetRequiredService<SettingsDbContext>());
            services.AddScoped<ISaveSmtpConfigCommand, SaveSmtpConfigCommand>();
            services.AddScoped<ITestSmtpEmailCommand, TestSmtpEmailCommand>();

            return services;
        }

        public WebApplication ConfigureModuleServices(WebApplication app, bool isDevelopment)
        {
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<SettingsDbContext>();
                dbContext.Database.Migrate();
                SettingsSeedData.Seed(dbContext);
            }

            return app;
        }
    }
}
