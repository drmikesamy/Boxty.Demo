using System.Security.Claims;
using Boxty.ServerBase.Auth.Constants;
using Boxty.ServerBase.Database;
using Boxty.ServerBase.Entities;
using Boxty.ServerBase.Interfaces;
using Boxty.ServerApp.Modules.UserManagement.Services;
using Boxty.SharedBase.DTOs;
using Boxty.SharedBase.Interfaces;
using FluentValidation;
using FS.Keycloak.RestApiClient.Model;

namespace Boxty.ServerApp.Modules.UserManagement.Infrastructure.Commands
{
    public interface ICreateTenantCommand<T, TDto, TContext>
    {
        Task<Guid> Handle(TDto dto, ClaimsPrincipal user);
    }

    public class CreateTenantCommand<T, TDto, TContext> : ICreateTenantCommand<T, TDto, TContext>, ICommand
        where T : class, IEntity, ITenantEntity
        where TDto : IDto, ITenant
        where TContext : IDbContext<TContext>
    {
        private readonly IKeycloakService _keycloakService;
        private readonly IValidator<TDto> _validator;

        public CreateTenantCommand(IKeycloakService keycloakService, IValidator<TDto> validator)
        {
            _keycloakService = keycloakService;
            _validator = validator;
        }

        public async Task<Guid> Handle(TDto dto, ClaimsPrincipal user)
        {
            try
            {
                var tenantName = dto.Name.Replace(" ", "-").ToLowerInvariant();
                await ValidateAndCacheAsync(dto, tenantName);

                var orgBody = new OrganizationRepresentation
                {
                    Name = tenantName,
                    Domains = new List<OrganizationDomainRepresentation>
                    {
                        new OrganizationDomainRepresentation { Name = dto.Domain }
                    },
                    Enabled = true
                };

                await _keycloakService.PostOrganizationAsync(orgBody);

                var newOrganizations = await _keycloakService.GetOrganizationsAsync(tenantName);
                var newId = newOrganizations?.FirstOrDefault()?.Id;

                if (string.IsNullOrEmpty(newId))
                {
                    throw new InvalidOperationException("Tenant was created in Keycloak but no organization ID was returned.");
                }

                dto.Id = Guid.Parse(newId);
                return dto.Id;
            }
            catch (ValidationException)
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
                throw new InvalidOperationException($"Failed to create tenant: {ex.Message}", ex);
            }
        }

        private async Task ValidateAndCacheAsync(TDto dto, string tenantName)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }

            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var tenantValidator = new TenantValidator<TDto>();
            var tenantValidationResult = await tenantValidator.ValidateAsync(dto);
            if (!tenantValidationResult.IsValid)
            {
                throw new ValidationException(tenantValidationResult.Errors);
            }

            var existingOrganizations = await _keycloakService.GetOrganizationsAsync(tenantName);
            if (existingOrganizations?.Any() == true)
            {
                throw new InvalidOperationException($"An organization with name '{dto.Name}' already exists in Keycloak.");
            }

            if (!string.IsNullOrEmpty(dto.Domain))
            {
                var allOrganizations = await _keycloakService.GetOrganizationsAsync(dto.Domain);

                var domainExists = allOrganizations?.Any(org =>
                    org.Domains?.Any(domain =>
                        string.Equals(domain.Name, dto.Domain, StringComparison.OrdinalIgnoreCase)) == true) == true;

                if (domainExists)
                {
                    throw new InvalidOperationException($"Domain '{dto.Domain}' is already in use by another organization in Keycloak.");
                }
            }
        }
    }

    public class TenantValidator<TDto> : AbstractValidator<TDto>
        where TDto : ITenant
    {
        public TenantValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name must not be empty.");

            RuleFor(x => x.Domain)
                .NotEmpty().WithMessage("Domain must not be empty.")
                .Matches(@"^(?:[a-zA-Z0-9-]+\.)+[a-zA-Z]{2,}$")
                .WithMessage("Domain must be a valid domain format.");
        }
    }
}