using System.Security.Claims;
using Boxty.ServerBase.Auth.Constants;
using Boxty.ServerBase.Endpoints;
using Boxty.ServerApp.Modules.Settings.Entities;
using Boxty.ServerApp.Modules.Settings.Infrastructure.Commands;
using Boxty.ServerApp.Modules.Settings.Infrastructure.Database;
using Boxty.SharedApp.DTOs.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Boxty.ServerApp.Modules.Settings.Endpoints
{
    public class SmtpConfigEndpoints : BaseEndpoints<SmtpConfig, SmtpConfigDto, SettingsDbContext>, IEndpoints
    {
        public override IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
        {
            base.MapEndpoints(endpoints);
            var permission = PermissionHelper.GeneratePermission<SmtpConfig>(PermissionOperations.Update);
            endpoints.MapPost("/api/SmtpConfig/TestEmail", (
                [FromServices] ITestSmtpEmailCommand command,
                TestSmtpEmailRequest request,
                CancellationToken cancellationToken
            ) => TestEmail(command, request, cancellationToken))
            .RequireAuthorization($"Permission:{permission}")
            .WithName("TestSmtpEmail")
            .WithSummary("Send a test email using supplied SMTP settings without saving them.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
            return endpoints;
        }

        protected Task<IResult> TestEmail(
            ITestSmtpEmailCommand command, TestSmtpEmailRequest request, CancellationToken cancellationToken) =>
            ExecuteWithValidation(async () =>
            {
                await command.Handle(request, cancellationToken);
                return Results.NoContent();
            }, "Unable to send the SMTP test email. Check the SMTP host, port, credentials, and recipient address.");

        protected override void MapCreateEndpoint(RouteGroupBuilder group)
        {
            var permission = PermissionHelper.GeneratePermission<SmtpConfig>(PermissionOperations.Create);
            group.MapPost("/Create", (
                [FromServices] ISaveSmtpConfigCommand command,
                ClaimsPrincipal user,
                SmtpConfigDto dto,
                CancellationToken cancellationToken
            ) => Create(command, user, dto, cancellationToken))
            .RequireAuthorization($"Permission:{permission}");
        }

        protected override void MapUpdateEndpoint(RouteGroupBuilder group)
        {
            var permission = PermissionHelper.GeneratePermission<SmtpConfig>(PermissionOperations.Update);
            group.MapPut("/Update", (
                [FromServices] ISaveSmtpConfigCommand command,
                ClaimsPrincipal user,
                SmtpConfigDto dto,
                CancellationToken cancellationToken
            ) => Update(command, user, dto, cancellationToken))
            .RequireAuthorization($"Permission:{permission}");
        }

        protected Task<IResult> Create(
            ISaveSmtpConfigCommand command, ClaimsPrincipal user, SmtpConfigDto dto,
            CancellationToken cancellationToken) =>
            ExecuteWithValidation(async () => Results.Ok(await command.Create(dto, user, cancellationToken)), SaveError);

        protected Task<IResult> Update(
            ISaveSmtpConfigCommand command, ClaimsPrincipal user, SmtpConfigDto dto,
            CancellationToken cancellationToken) =>
            ExecuteWithValidation(async () => Results.Ok(await command.Update(dto, user, cancellationToken)), SaveError);

        private const string SaveError =
            "Unable to save and synchronize SMTP settings with Keycloak. Reload the settings and retry; if the problem persists, contact an administrator.";
    }
}
