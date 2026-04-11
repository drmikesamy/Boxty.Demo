using Boxty.ServerBase.Mappers;
using Boxty.ServerBase.Queries.ModuleQueries;
using Boxty.ServerApp.Modules.UserManagement.Entities;
using Boxty.ServerApp.Modules.UserManagement.Infrastructure.Database;
using Boxty.SharedBase.DTOs.Auth;
using Microsoft.EntityFrameworkCore;

namespace Boxty.ServerApp.Modules.UserManagement.Infrastructure.Queries
{
    public class GetAllRolesWithPermissionsQuery : IGetAllRolesWithPermissionsQuery
    {
        private readonly UserManagementDbContext _dbContext;
        private readonly IMapper<Role, RoleDto> _mapper;

        public GetAllRolesWithPermissionsQuery(UserManagementDbContext dbContext, IMapper<Role, RoleDto> mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<IEnumerable<RoleDto>> Handle()
        {
            var roles = await _dbContext.Roles
                .Include(r => r.Permissions)
                .AsNoTracking()
                .ToListAsync();

            return _mapper.Map(roles).ToList();
        }
    }
}