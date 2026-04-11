using System.Collections.Generic;
using System.Linq;
using Boxty.ServerBase.Mappers;
using Boxty.ServerApp.Modules.UserManagement.Entities;
using Boxty.SharedBase.DTOs.Auth;
using System.Security.Claims;

namespace Boxty.ServerApp.Modules.UserManagement.Infrastructure.Mappers
{
    public class PermissionMapper : IMapper<Permission, PermissionDto>
    {
        public PermissionDto Map(Permission entity, ClaimsPrincipal? user = null)
        {
            return new PermissionDto
            {
                Id = entity.Id,
                Name = entity.Name
            };
        }

        public Permission Map(PermissionDto dto, ClaimsPrincipal? user = null)
        {
            return new Permission
            {
                Id = dto.Id,
                Name = dto.Name
            };
        }

        public IEnumerable<PermissionDto> Map(IEnumerable<Permission> entities, ClaimsPrincipal? user = null)
        {
            return entities.Select(entity => Map(entity, user));
        }

        public IEnumerable<Permission> Map(IEnumerable<PermissionDto> dtos, ClaimsPrincipal? user = null)
        {
            return dtos.Select(dto => Map(dto, user)).ToList();
        }

        public void Map(PermissionDto dto, Permission entity, ClaimsPrincipal? user = null)
        {
            entity.Name = dto.Name;
        }

        public void Map(Permission entity, PermissionDto dto, ClaimsPrincipal? user = null)
        {
            dto.Id = entity.Id;
            dto.Name = entity.Name;
        }
    }
}
