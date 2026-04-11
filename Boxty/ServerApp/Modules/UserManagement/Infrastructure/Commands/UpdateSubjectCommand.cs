using System.Security.Claims;
using Boxty.ServerBase.Commands;
using Boxty.ServerBase.Database;
using Boxty.ServerBase.Entities;
using Boxty.ServerBase.Interfaces;
using Boxty.ServerApp.Modules.UserManagement.Services;
using Boxty.SharedBase.DTOs;
using Boxty.SharedBase.Interfaces;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;

namespace Boxty.ServerApp.Modules.UserManagement.Infrastructure.Commands
{
    public interface IUpdateSubjectCommand<T, TDto, TContext>
    {
        Task<Guid> Handle(TDto dto, ClaimsPrincipal user);
    }

    public class UpdateSubjectCommand<T, TDto, TContext> : IUpdateSubjectCommand<T, TDto, TContext>, ICommand
        where T : class, IEntity, ISubjectEntity
        where TDto : IDto, IAuditDto, ISubject
        where TContext : IDbContext<TContext>
    {
        private readonly IDbContext<TContext> _dbContext;
        private readonly IUpdateCommand<T, TDto, TContext> _updateCommand;
        private readonly IKeycloakService _keycloakService;
        private readonly IUpdateUserRolesCommand _updateUserRolesCommand;
        private readonly IUserClaimsReader _userClaimsReader;

        public UpdateSubjectCommand(
            IDbContext<TContext> dbContext,
            IUpdateCommand<T, TDto, TContext> updateCommand,
            IKeycloakService keycloakService,
            IUpdateUserRolesCommand updateUserRolesCommand,
            IUserClaimsReader userClaimsReader)
        {
            _dbContext = dbContext;
            _updateCommand = updateCommand;
            _keycloakService = keycloakService;
            _updateUserRolesCommand = updateUserRolesCommand;
            _userClaimsReader = userClaimsReader;
        }

        public async Task<Guid> Handle(TDto dto, ClaimsPrincipal user)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }

            var existingSubject = await _dbContext.Set<T>()
                .AsNoTracking()
                .SingleOrDefaultAsync(subject => subject.Id == dto.Id);

            if (existingSubject == null)
            {
                throw new KeyNotFoundException($"Subject with ID {dto.Id} not found.");
            }

            var requestedRoleName = string.IsNullOrWhiteSpace(dto.RoleName)
                ? "subject"
                : KeycloakAdminCommandHelper.NormalizeRoleName(dto.RoleName);

            dto.RoleName = requestedRoleName;

            var validationErrors = await ValidateAsync(dto, user, existingSubject, requestedRoleName);
            if (validationErrors.Any())
            {
                throw new ValidationException(validationErrors);
            }

            var updatedId = await _updateCommand.Handle(dto, user);

            var keycloakUser = await _keycloakService.GetUserByIdAsync(updatedId.ToString())
                ?? throw new InvalidOperationException($"User with ID '{updatedId}' not found in Keycloak.");

            keycloakUser.FirstName = dto.FirstName;
            keycloakUser.LastName = dto.LastName;
            keycloakUser.Username = dto.Email;
            keycloakUser.Email = dto.Email;
            keycloakUser.Enabled = dto.IsActive;

            await _keycloakService.UpdateUserAsync(updatedId.ToString(), keycloakUser);
            await _updateUserRolesCommand.Handle(updatedId, new List<string> { dto.RoleName ?? "subject" }, user);

            return updatedId;
        }

        private async Task<List<ValidationFailure>> ValidateAsync(TDto dto, ClaimsPrincipal user, T existingSubject, string requestedRoleName)
        {
            var validationErrors = SubjectAuthorizationHelper.ValidateRoleAssignment(
                _userClaimsReader,
                user,
                requestedRoleName,
                dto.TenantId,
                "You are not authorised to manage subject roles.",
                "manage subjects");

            try
            {
                await KeycloakAdminCommandHelper.EnsureUserExistsAsync(_keycloakService, dto.Id);
            }
            catch (Exception ex)
            {
                validationErrors.Add(new ValidationFailure("Id", $"Failed to verify subject in Keycloak: {ex.Message}"));
            }

            try
            {
                await _keycloakService.GetRoleByNameAsync(requestedRoleName);
            }
            catch (Exception ex) when (KeycloakAdminCommandHelper.IsNotFound(ex))
            {
                validationErrors.Add(new ValidationFailure("RoleName", $"Role '{requestedRoleName}' does not exist in Keycloak."));
            }
            catch (Exception ex)
            {
                validationErrors.Add(new ValidationFailure("RoleName", $"Failed to verify role '{requestedRoleName}' in Keycloak: {ex.Message}"));
            }

            if (!string.Equals(existingSubject.Email, dto.Email, StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var matchingUsers = await _keycloakService.GetUsersAsync(dto.Email, 10);
                    if (matchingUsers.Any(userRepresentation => !string.Equals(userRepresentation.Id, dto.Id.ToString(), StringComparison.OrdinalIgnoreCase)))
                    {
                        validationErrors.Add(new ValidationFailure("Email", $"A user with email '{dto.Email}' already exists in Keycloak."));
                    }
                }
                catch (Exception ex)
                {
                    validationErrors.Add(new ValidationFailure("Email", $"Failed to verify email availability: {ex.Message}"));
                }
            }

            return validationErrors;
        }
    }
}