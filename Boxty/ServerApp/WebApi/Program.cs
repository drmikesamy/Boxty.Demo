using Boxty.ServerBase.Extensions;
using Boxty.ServerApp.Modules.UserManagement.Infrastructure;
using Boxty.ServerApp.Modules.Settings.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var moduleTypes = new List<Type>
{
    typeof(SettingsModule),
    typeof(UserManagementModule)
};

builder.Services.RegisterServices(builder.Configuration, ref moduleTypes, out var registeredModules);

var app = builder.Build();

app.ConfigureServicesAndMapEndpoints(builder.Environment.IsDevelopment() || builder.Environment.IsStaging(), registeredModules);
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

app.Run();