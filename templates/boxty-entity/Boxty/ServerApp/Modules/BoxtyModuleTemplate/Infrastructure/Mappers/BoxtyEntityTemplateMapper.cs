using Boxty.ServerBase.Mappers;
using ModuleEntity = Boxty.ServerApp.Modules.BoxtyModuleTemplate.Entities.BoxtyEntityTemplate;
using Boxty.SharedApp.DTOs.BoxtyModuleTemplate;
using System.Security.Claims;

namespace Boxty.ServerApp.Modules.BoxtyModuleTemplate.Infrastructure.Mappers
{
    public class BoxtyEntityTemplateMapper : IMapper<ModuleEntity, BoxtyEntityTemplateDto>
    {
        public BoxtyEntityTemplateDto Map(ModuleEntity entity, ClaimsPrincipal? user = null)
        {
            return new BoxtyEntityTemplateDto
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

        public ModuleEntity Map(BoxtyEntityTemplateDto dto, ClaimsPrincipal? user = null)
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

        public void Map(BoxtyEntityTemplateDto dto, ModuleEntity entity, ClaimsPrincipal? user = null)
        {
            entity.Id = dto.Id;
            entity.Name = dto.Name;
            entity.Description = dto.Description;
            entity.CreatedDate = dto.CreatedDate;
            entity.CreatedBy = dto.CreatedBy;
            entity.ModifiedDate = dto.ModifiedDate;
            entity.LastModifiedBy = dto.LastModifiedBy;
        }

        public void Map(ModuleEntity entity, BoxtyEntityTemplateDto dto, ClaimsPrincipal? user = null)
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
