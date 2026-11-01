using Boxty.ServerBase.Extensions;
using Boxty.ServerApp.Modules.UserManagement.Infrastructure;
using Boxty.ServerApp.Modules.Settings.Infrastructure;
using Boxty.ServerApp.Modules.DemoModule.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var moduleTypes = new List<Type>
{
    typeof(SettingsModule),
    typeof(UserManagementModule),
    typeof(DemoModuleModule)
};

builder.Services.RegisterServices(builder.Configuration, ref moduleTypes, out var registeredModules);

var app = builder.Build();

app.ConfigureServicesAndMapEndpoints(builder.Environment.IsDevelopment() || builder.Environment.IsStaging(), registeredModules);
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

app.Run();