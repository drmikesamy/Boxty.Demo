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
using Microsoft.Extensions.Options;

namespace Boxty.ServerApp.Modules.UserManagement.Infrastructure.Commands
{
    public interface ICreateSubjectCommand<T, TDto, TContext>
    {
        Task<Guid> Handle(TDto dto, ClaimsPrincipal user);
    }

    public class CreateSubjectCommand<T, TDto, TContext> : ICreateSubjectCommand<T, TDto, TContext>, ICommand
        where T : class, IEntity, ISubjectEntity
        where TDto : IDto, IAuditDto, ISubject
        where TContext : IDbContext<TContext>
    {
        private readonly ICreateCommand<T, TDto, TContext> _createCommand;
        private readonly IKeycloakService _keycloakService;
        private readonly IValidator<TDto> _validator;
        private readonly ISendEmailCommand _sendEmailCommand;
        private readonly AppOptions _options;

        public CreateSubjectCommand(
            ICreateCommand<T, TDto, TContext> createCommand,
            IKeycloakService keycloakService,
            IValidator<TDto> validator,
            ISendEmailCommand sendEmailCommand,
            IOptions<AppOptions> options)
        {
            _createCommand = createCommand;
            _keycloakService = keycloakService;
            _validator = validator;
            _sendEmailCommand = sendEmailCommand;
            _options = options.Value;
        }

        public async Task<Guid> Handle(TDto dto, ClaimsPrincipal user)
        {
            string? createdKeycloakUserId = null;

            try
            {
                var validationContext = await ValidateAndCacheAsync(dto, user);
                var newTemporaryPassword = PasswordHelper.GenerateTemporaryPassword();

                var userRep = new UserRepresentation
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Username = dto.Email,
                    Email = dto.Email,
                    Credentials = new List<CredentialRepresentation>
                    {
                        new CredentialRepresentation
                        {
                            UserLabel = "Password",
                            Type = "password",
                            Value = newTemporaryPassword,
                            Temporary = true
                        }
                    },
                    Enabled = true
                };

                await _keycloakService.PostUsersAsync(userRep);

                var existingUsers = await _keycloakService.GetUsersAsync(dto.Email, 1);
                var newId = existingUsers?.FirstOrDefault()?.Id;
                if (string.IsNullOrEmpty(newId))
                {
                    throw new InvalidOperationException("Subject was created in Keycloak but no user ID was returned.");
                }

                createdKeycloakUserId = newId;

                await _keycloakService.PostOrganizationMemberAsync(dto.TenantId.ToString(), newId);
                await _keycloakService.PostUserRoleMappingAsync(newId, new List<RoleRepresentation> { validationContext.ValidatedRole });

                dto.Id = Guid.Parse(newId);
                await _createCommand.Handle(dto, user);
                await SendWelcomeEmailAsync(dto, newTemporaryPassword, user);

                return dto.Id;
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
                if (!string.IsNullOrWhiteSpace(createdKeycloakUserId))
                {
                    await TryDeleteKeycloakUserAsync(createdKeycloakUserId);
                }

                throw new InvalidOperationException($"Failed to create subject: {ex.Message}", ex);
            }
        }

        private async Task SendWelcomeEmailAsync(TDto dto, string newTemporaryPassword, ClaimsPrincipal user)
        {
            try
            {
                if (!_options.Email.EnableEmailSending)
                {
                    return;
                }

                var subject = "Boxty Account Created";
                var htmlContent = $@"
                    <html>
                    <body>
                        <p>Welcome to the Boxty Portal {dto.FirstName} {dto.LastName},</p>
                        <p>Your portal account has been successfully created and can be accessed through the following link: <a href=""https://boxty.com"">Boxty - Home</a></p>
                        <p>Please use the following credentials to log in:</p>
                        <p><strong>Email:</strong> {dto.Email}</p>
                        <p><strong>Temporary Password:</strong> {newTemporaryPassword}</p>
                        <p><strong>Role:</strong> {dto.RoleName ?? "Subject"}</p>
                        <div style=""text-align: center; margin: 30px 0;""><a href=""https://boxty.org"" style=""display: inline-block; background-color: #007bff; color: white; text-decoration: none; padding: 12px 24px; border-radius: 5px; font-weight: bold; font-size: 16px; border: none; cursor: pointer;"">Go to Boxty</a></div>
                        <p>Please note you will be prompted to change your password when you first log in. Your portal login requires two factor authentication, please ensure you have downloaded an authenticator app such as Microsoft Authenticator to allow you to login. If you have any questions, please contact us at <a href=""mailto:admin@boxty.co.uk"">info@boxty.com</a></p>
                        <br/>
                        <p>Kind Regards,<br/>Boxty</p>
                    </body>
                    </html>";

                var plainTextContent = $@"
Welcome to the Boxty Portal {dto.FirstName} {dto.LastName},

Your portal account has been successfully created and can be accessed through the following link: Boxty - Home (https://boxty.com)

Please use the following credentials to log in:

Email: {dto.Email}
Temporary Password: {newTemporaryPassword}
Role: {dto.RoleName ?? "Subject"}

Please note you will be prompted to change your password when you first log in. Your portal login requires two factor authentication, please ensure you have downloaded an authenticator app such as Microsoft Authenticator to allow you to login. If you have any questions, please contact us at admin@boxty.co.uk or alternatively call 01147004362

Kind Regards,
Boxty";

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
            catch (Exception)
            {
                // Email delivery should not fail subject creation; failures are non-blocking.
            }
        }

        private async Task TryDeleteKeycloakUserAsync(string userId)
        {
            try
            {
                await _keycloakService.DeleteUserAsync(userId);
            }
            catch
            {
            }
        }

        private async Task<ValidationContext> ValidateAndCacheAsync(TDto dto, ClaimsPrincipal user)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }

            var validationErrors = new List<FluentValidation.Results.ValidationFailure>();
            var context = new ValidationContext();

            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                validationErrors.AddRange(validationResult.Errors);
            }

            try
            {
                var existingUsers = await _keycloakService.GetUsersAsync(dto.Email, 1);
                if (existingUsers?.Any() == true)
                {
                    validationErrors.Add(new FluentValidation.Results.ValidationFailure("Email", $"A user with email '{dto.Email}' already exists in Keycloak."));
                }
            }
            catch (Exception ex)
            {
                validationErrors.Add(new FluentValidation.Results.ValidationFailure("Email", $"Failed to verify email availability: {ex.Message}"));
            }

            if (dto.TenantId != Guid.Empty)
            {
                try
                {
                    context.ValidatedOrganization = await _keycloakService.GetOrganizationByIdAsync(dto.TenantId.ToString());
                }
                catch (Exception ex) when (ex.Message.Contains("404") || ex.Message.Contains("Not Found"))
                {
                    validationErrors.Add(new FluentValidation.Results.ValidationFailure("TenantId", $"Organization with ID '{dto.TenantId}' does not exist in Keycloak."));
                }
                catch (Exception ex)
                {
                    validationErrors.Add(new FluentValidation.Results.ValidationFailure("TenantId", $"Failed to verify organization '{dto.TenantId}' in Keycloak: {ex.Message}"));
                }
            }

            if (string.IsNullOrEmpty(dto.RoleName))
            {
                dto.RoleName = "subject";
            }

            try
            {
                context.ValidatedRole = await _keycloakService.GetRoleByNameAsync(dto.RoleName);
            }
            catch (Exception ex) when (ex.Message.Contains("404") || ex.Message.Contains("Not Found"))
            {
                validationErrors.Add(new FluentValidation.Results.ValidationFailure("RoleName", $"Role '{dto.RoleName}' does not exist in Keycloak."));
            }
            catch (Exception ex)
            {
                validationErrors.Add(new FluentValidation.Results.ValidationFailure("RoleName", $"Failed to verify role '{dto.RoleName}' in Keycloak: {ex.Message}"));
            }

            if (validationErrors.Any())
            {
                throw new ValidationException(validationErrors);
            }

            return context;
        }

        private class ValidationContext
        {
            public OrganizationRepresentation? ValidatedOrganization { get; set; }
            public RoleRepresentation ValidatedRole { get; set; } = null!;
        }
    }
}