using System.Security.Claims;
using Boxty.ServerBase.Database;
using Boxty.ServerBase.Entities;
using Boxty.ServerBase.Interfaces;
using Boxty.ServerApp.Modules.UserManagement.Services;
using Boxty.SharedBase.DTOs;
using Boxty.SharedBase.Interfaces;

namespace Boxty.ServerApp.Modules.UserManagement.Infrastructure.Commands
{
    public interface IDeleteTenantCommand<T, TDto, TContext>
    {
        Task<bool> Handle(Guid id, ClaimsPrincipal user);
    }

    public class DeleteTenantCommand<T, TDto, TContext> : IDeleteTenantCommand<T, TDto, TContext>, ICommand
        where T : class, IEntity, ITenantEntity
        where TDto : IDto, ITenant
        where TContext : IDbContext<TContext>
    {
        private readonly IKeycloakService _keycloakService;

        public DeleteTenantCommand(IKeycloakService keycloakService)
        {
            _keycloakService = keycloakService;
        }

        public async Task<bool> Handle(Guid id, ClaimsPrincipal user)
        {
            await _keycloakService.DeleteOrganizationAsync(id.ToString());
            return true;
        }
    }
}