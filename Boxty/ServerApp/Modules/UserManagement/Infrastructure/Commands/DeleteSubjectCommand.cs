using System.Security.Claims;
using Boxty.ServerBase.Database;
using Boxty.ServerBase.Entities;
using Boxty.ServerBase.Interfaces;
using Boxty.ServerApp.Modules.UserManagement.Services;
using Boxty.SharedBase.DTOs;
using Boxty.SharedBase.Interfaces;

namespace Boxty.ServerApp.Modules.UserManagement.Infrastructure.Commands
{
    public interface IDeleteSubjectCommand<T, TDto, TContext>
    {
        Task<bool> Handle(Guid id, ClaimsPrincipal user);
    }

    public class DeleteSubjectCommand<T, TDto, TContext> : IDeleteSubjectCommand<T, TDto, TContext>, ICommand
        where T : class, IEntity, ISubjectEntity
        where TDto : IDto, IAuditDto, ISubject
        where TContext : IDbContext<TContext>
    {
        private readonly IKeycloakService _keycloakService;

        public DeleteSubjectCommand(IKeycloakService keycloakService)
        {
            _keycloakService = keycloakService;
        }

        public async Task<bool> Handle(Guid id, ClaimsPrincipal user)
        {
            await _keycloakService.DeleteUserAsync(id.ToString());
            return true;
        }
    }
}