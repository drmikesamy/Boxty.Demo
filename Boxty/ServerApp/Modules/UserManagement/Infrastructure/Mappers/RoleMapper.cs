using System.Collections.Generic;
using System.Linq;
using Boxty.ServerBase.Mappers;
using Boxty.ServerApp.Modules.UserManagement.Entities;
using Boxty.ServerApp.Modules.UserManagement.Infrastructure.Database;
using Boxty.SharedBase.DTOs.Auth;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace Boxty.ServerApp.Modules.UserManagement.Infrastructure.Mappers
{
    public class RoleMapper : IMapper<Role, RoleDto>
    {
        private readonly UserManagementDbContext _dbContext;

        public RoleMapper(UserManagementDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public RoleDto Map(Role entity, ClaimsPrincipal? user = null)
        {
            var permissions = entity.Permissions ?? new List<Permission>();
            return new RoleDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Permissions = permissions.Select(MapPermissionToDto).ToList()
            };
        }

        public Role Map(RoleDto dto, ClaimsPrincipal? user = null)
        {
            return new Role
            {
                Id = dto.Id,
                Name = dto.Name,
                Permissions = ResolvePermissions(dto.Permissions)
            };
        }

        public IEnumerable<RoleDto> Map(IEnumerable<Role> entities, ClaimsPrincipal? user = null)
        {
            return entities.Select(entity => Map(entity, user));
        }

        public IEnumerable<Role> Map(IEnumerable<RoleDto> dtos, ClaimsPrincipal? user = null)
        {
            return dtos.Select(dto => Map(dto, user)).ToList();
        }

        public void Map(RoleDto dto, Role entity, ClaimsPrincipal? user = null)
        {
            entity.Name = dto.Name;

            _dbContext.Entry(entity).Collection(r => r.Permissions).Load();
            entity.Permissions.Clear();

            foreach (var permission in ResolvePermissions(dto.Permissions))
            {
                entity.Permissions.Add(permission);
            }
        }

        public void Map(Role entity, RoleDto dto, ClaimsPrincipal? user = null)
        {
            dto.Id = entity.Id;
            dto.Name = entity.Name;
            dto.Permissions = (entity.Permissions ?? new List<Permission>()).Select(MapPermissionToDto).ToList();
        }

        public static RoleDto MapToDto(Role role, IEnumerable<Permission> permissions)
        {
            return new RoleDto
            {
                Id = role.Id,
                Name = role.Name,
                Permissions = permissions.Select(MapPermissionToDto).ToList()
            };
        }

        public static PermissionDto MapPermissionToDto(Permission permission)
        {
            return new PermissionDto
            {
                Id = permission.Id,
                Name = permission.Name
            };
        }

        private List<Permission> ResolvePermissions(IEnumerable<PermissionDto>? permissionDtos)
        {
            if (permissionDtos == null)
            {
                return new List<Permission>();
            }

            var permissionIds = permissionDtos
                .Where(p => p.Id != Guid.Empty)
                .Select(p => p.Id)
                .Distinct()
                .ToList();

            if (permissionIds.Count == 0)
            {
                return new List<Permission>();
            }

            return _dbContext.Permissions
                .Where(p => permissionIds.Contains(p.Id))
                .ToList();
        }
    }
}
