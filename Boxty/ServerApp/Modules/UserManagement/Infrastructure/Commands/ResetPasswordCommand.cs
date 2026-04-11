using System.Security.Claims;
using Boxty.ServerBase.Commands;
using Boxty.ServerBase.Config;
using Boxty.ServerBase.Database;
using Boxty.ServerBase.Entities;
using Boxty.ServerBase.Interfaces;
using Boxty.ServerBase.Models.Email;
using Boxty.ServerApp.Modules.UserManagement.Services;
using Boxty.SharedBase.DTOs;
using Boxty.SharedBase.Helpers;
using Boxty.SharedBase.Interfaces;
using FluentValidation;
using FS.Keycloak.RestApiClient.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Boxty.ServerApp.Modules.UserManagement.Infrastructure.Commands
{
    public interface IResetPasswordCommand<T, TDto, TContext>
    {
        Task<TDto> Handle(Guid id, ClaimsPrincipal user);
    }

    public class ResetPasswordCommand<T, TDto, TContext> : IResetPasswordCommand<T, TDto, TContext>, ICommand
        where T : class, IEntity, ISubjectEntity
        where TDto : IDto, IAuditDto, ISubject
        where TContext : IDbContext<TContext>
    {
        private IDbContext<TContext> DbContext { get; }
        private readonly IKeycloakService _keycloakService;
        private readonly IUserClaimsReader _userClaimsReader;
        private readonly ISendEmailCommand _sendEmailCommand;
        private readonly AppOptions _options;

        public ResetPasswordCommand(
            IDbContext<TContext> dbContext,
            IKeycloakService keycloakService,
            IUserClaimsReader userClaimsReader,
            ISendEmailCommand sendEmailCommand,
            IOptions<AppOptions> options)
        {
            DbContext = dbContext;
            _keycloakService = keycloakService;
            _userClaimsReader = userClaimsReader;
            _sendEmailCommand = sendEmailCommand;
            _options = options.Value;
        }

        public async Task<TDto> Handle(Guid id, ClaimsPrincipal user)
        {
            try
            {
                var entity = await DbContext.Set<T>().FirstOrDefaultAsync(e => e.Id == id);
                if (entity == null)
                {
                    throw new InvalidOperationException($"Subject with ID '{id}' not found.");
                }

                ValidatePasswordResetAuthorization(user, entity);

                var newTemporaryPassword = PasswordHelper.GenerateTemporaryPassword();
                var credentialRepresentation = new CredentialRepresentation
                {
                    UserLabel = "Password",
                    Type = "password",
                    Value = newTemporaryPassword,
                    Temporary = true
                };

                await _keycloakService.ResetUserPasswordAsync(id.ToString(), credentialRepresentation);

                var keycloakUser = await _keycloakService.GetUserByIdAsync(id.ToString());
                if (keycloakUser == null)
                {
                    throw new InvalidOperationException($"User with ID '{id}' not found in Keycloak.");
                }

                var dto = new
                {
                    Id = id,
                    FirstName = entity.FirstName ?? string.Empty,
                    LastName = entity.LastName ?? string.Empty,
                    Email = entity.Email ?? string.Empty,
                    RoleName = entity.RoleName
                };

                await SendPasswordResetEmailAsync(dto, newTemporaryPassword, user);

                var result = Activator.CreateInstance<TDto>();
                result.Id = id;
                if (result is ISubject subjectResult)
                {
                    subjectResult.FirstName = dto.FirstName;
                    subjectResult.LastName = dto.LastName;
                    subjectResult.Email = dto.Email;
                    subjectResult.RoleName = dto.RoleName;
                }

                return result;
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (ArgumentNullException ex)
            {
                throw new InvalidOperationException($"Invalid input: {ex.ParamName} cannot be null", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to reset password: {ex.Message}", ex);
            }
        }

        private async Task SendPasswordResetEmailAsync(dynamic dto, string newTemporaryPassword, ClaimsPrincipal user)
        {
            try
            {
                if (!_options.Email.EnableEmailSending)
                {
                    return;
                }

                var subject = "Boxty Portal - Password Reset Notification";
                var htmlContent = $@"
                    <html>
                    <body>
                        <p>Dear {dto.FirstName} {dto.LastName},</p>
                        <p>Your password has been reset. Please use the following credentials to log in:</p>
                        <p><strong>Email:</strong> {dto.Email}</p>
                        <p><strong>Temporary Password:</strong> {newTemporaryPassword}</p>
                        <p><strong>Role:</strong> {dto.RoleName ?? "Subject"}</p>
                        <div style=""text-align: center; margin: 30px 0;""><a href=""https://boxty.org"" style=""display: inline-block; background-color: #007bff; color: white; text-decoration: none; padding: 12px 24px; border-radius: 5px; font-weight: bold; font-size: 16px; border: none; cursor: pointer;"">Go to Boxty</a></div>
                        <p>Please note you will be prompted to change your password when you log in and two factor authentication is required. If you have any questions, please contact us at <a href=""mailto:admin@boxty.co.uk"">info@boxty.com</a>.</p>
                        <br/>
                        <p>Kind Regards,<br/>Boxty</p>
                    </body>
                    </html>";

                var plainTextContent = $@"
Dear {dto.FirstName} {dto.LastName},

Your password has been reset. Please use the following credentials to log in:

Email: {dto.Email}
Temporary Password: {newTemporaryPassword}
Role: {dto.RoleName ?? "Subject"}

Please note you will be prompted to change your password when you log in and two factor authentication is required. If you have any questions, please contact us at admin@boxty.co.uk or alternatively call 01147004362.

Kind Regards,
Boxty

Telephone - 01147004362
Email - admin@boxty.co.uk

This is an automated email notification from the Boxty Portal. Please do not reply to this email.";

                var emailRequest = new SendEmailRequest
                {
                    SenderAddress = _options.Email.SenderAddress,
                    RecipientAddress = dto.Email,
                    Subject = subject,
                    HtmlContent = htmlContent,
                    PlainTextContent = plainTextContent,
                    IsHighPriority = false
                };

                await _sendEmailCommand.Handle(emailRequest, user);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Password reset successfully, but failed to send notification email: {ex.Message}", ex);
            }
        }

        private void ValidatePasswordResetAuthorization(ClaimsPrincipal user, T targetEntity)
        {
            SubjectAuthorizationHelper.EnsureCanResetPassword(_userClaimsReader, user, targetEntity.RoleName);
        }
    }
}