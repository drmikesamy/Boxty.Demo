using Boxty.ServerBase.Mappers;
using ModuleEntity = Boxty.ServerApp.Modules.DemoModule.Entities.DemoEntity;
using Boxty.SharedApp.DTOs.DemoModule;
using System.Security.Claims;

namespace Boxty.ServerApp.Modules.DemoModule.Infrastructure.Mappers
{
    public class DemoEntityMapper : IMapper<ModuleEntity, DemoEntityDto>
    {
        public DemoEntityDto Map(ModuleEntity entity, ClaimsPrincipal? user = null)
        {
            return new DemoEntityDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                CreatedDate = entity.CreatedDate,
                CreatedBy = entity.CreatedBy,
                ModifiedDate = entity.ModifiedDate,
                LastModifiedBy = entity.LastModifiedBy
            };
        }

        public ModuleEntity Map(DemoEntityDto dto, ClaimsPrincipal? user = null)
        {
            return new ModuleEntity
            {
                Id = dto.Id,
                Name = dto.Name,
                Description = dto.Description,
                CreatedDate = dto.CreatedDate,
                CreatedBy = dto.CreatedBy,
                ModifiedDate = dto.ModifiedDate,
                LastModifiedBy = dto.LastModifiedBy
            };
        }

        public void Map(DemoEntityDto dto, ModuleEntity entity, ClaimsPrincipal? user = null)
        {
            entity.Id = dto.Id;
            entity.Name = dto.Name;
            entity.Description = dto.Description;
            entity.CreatedDate = dto.CreatedDate;
            entity.CreatedBy = dto.CreatedBy;
            entity.ModifiedDate = dto.ModifiedDate;
            entity.LastModifiedBy = dto.LastModifiedBy;
        }

        public void Map(ModuleEntity entity, DemoEntityDto dto, ClaimsPrincipal? user = null)
        {
            dto.Id = entity.Id;
            dto.Name = entity.Name;
            dto.Description = entity.Description;
            dto.CreatedDate = entity.CreatedDate;
            dto.CreatedBy = entity.CreatedBy;
            dto.ModifiedDate = entity.ModifiedDate;
            dto.LastModifiedBy = entity.LastModifiedBy;
        }
    }
}