using Blazor.SubtleCrypto;
using Boxty.ClientApp.Configuration;
using Boxty.ClientApp.RoleManagement;
using Boxty.ClientBase.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using MudBlazor.Services;
using Boxty.SharedApp.DTOs.UserManagement;
using FluentValidation;
using System.Text.Json;
using System.Text.Json.Serialization;
using Boxty.SharedApp.Validators;
using Boxty.ClientApp;
using Boxty.SharedBase.DTOs.Auth;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure options from appsettings.json
builder.Services.Configure<ApiOptions>(builder.Configuration.GetSection(ApiOptions.SectionName));
builder.Services.Configure<OidcOptions>(builder.Configuration.GetSection(OidcOptions.SectionName));
builder.Services.Configure<CultureOptions>(builder.Configuration.GetSection(CultureOptions.SectionName));
builder.Services.Configure<ApplicationSettingsOptions>(builder.Configuration.GetSection(ApplicationSettingsOptions.SectionName));

// Get configuration values
var apiOptions = builder.Configuration.GetSection(ApiOptions.SectionName).Get<ApiOptions>() ?? new ApiOptions();
var oidcOptions = builder.Configuration.GetSection(OidcOptions.SectionName).Get<OidcOptions>() ?? new OidcOptions();
var cultureOptions = builder.Configuration.GetSection(CultureOptions.SectionName).Get<CultureOptions>() ?? new CultureOptions();
var applicationSettingsOptions = builder.Configuration.GetSection(ApplicationSettingsOptions.SectionName).Get<ApplicationSettingsOptions>() ?? new ApplicationSettingsOptions();

// Configure JSON options for System.Text.Json in Blazor WebAssembly
builder.Services.Configure<JsonSerializerOptions>(options =>
{
    options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.PropertyNameCaseInsensitive = true;
    options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    // Polymorphic serialization is handled automatically by the JsonPolymorphic attributes
});

builder.Services.AddScoped<AuthHttpMessageHandler>();

builder.Services.AddHttpClient("api", (sp, client) =>
{
    client.BaseAddress = new Uri(apiOptions.BaseUrl);
    // Configure default JSON serialization
    client.DefaultRequestHeaders.Accept.Clear();
    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
})
   .AddHttpMessageHandler(sp =>
   {
       var handler = sp.GetRequiredService<AuthorizationMessageHandler>()
           .ConfigureHandler(authorizedUrls: apiOptions.AuthorizedUrls);
       return handler;
   })
   .AddHttpMessageHandler<AuthHttpMessageHandler>();

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("api"));

builder.Services.AddOidcAuthentication(options =>
{
    options.ProviderOptions.Authority = oidcOptions.Authority;
    options.ProviderOptions.ClientId = oidcOptions.ClientId;
    options.ProviderOptions.ResponseType = oidcOptions.ResponseType;
    foreach (var scope in oidcOptions.DefaultScopes)
    {
        options.ProviderOptions.DefaultScopes.Add(scope);
    }
    options.UserOptions.RoleClaim = oidcOptions.RoleClaim;
}).AddAccountClaimsPrincipalFactory<ParseRoleClaimsPrincipalFactory>();

builder.Services.AddMudServices();
MudGlobal.InputDefaults.Variant = Variant.Outlined;
builder.Services.AddSubtleCrypto();

// Register FluentValidation validators so they can be injected in Blazor components
builder.Services.AddValidatorsFromAssemblyContaining<SubjectValidator>();

// Register application services
builder.Services.AddScoped<ICrudService<TenantDto>, CrudService<TenantDto>>();
builder.Services.AddScoped<ICrudService<SubjectDto>, CrudService<SubjectDto>>();
builder.Services.AddScoped<ICrudService<TenantDocumentDto>, CrudService<TenantDocumentDto>>();
builder.Services.AddScoped<ICrudService<SubjectDocumentDto>, CrudService<SubjectDocumentDto>>();
builder.Services.AddScoped<ICrudService<TenantNoteDto>, CrudService<TenantNoteDto>>();
builder.Services.AddScoped<ICrudService<SubjectNoteDto>, CrudService<SubjectNoteDto>>();
builder.Services.AddScoped<ICrudService<RoleDto>, CrudService<RoleDto>>();

builder.Services.AddScoped<IDocumentUploadService, DocumentUploadService>();
builder.Services.AddScoped<IAuthHelperService, AuthHelperService>();
builder.Services.AddScoped<GlobalStateService>();

var culture = new System.Globalization.CultureInfo(cultureOptions.DefaultCulture);
System.Globalization.CultureInfo.DefaultThreadCurrentCulture = culture;
System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = culture;

await builder.Build().RunAsync();
