using Boxty.ServerBase.Mappers;
using Boxty.ServerApp.Modules.BoxtyModuleTemplate.Entities;
using Boxty.SharedApp.DTOs.BoxtyModuleTemplate;
using System.Security.Claims;

namespace Boxty.ServerApp.Modules.BoxtyModuleTemplate.Infrastructure.Mappers
{
    public class BoxtyEntityTemplateMapper : IMapper<BoxtyEntityTemplate, BoxtyEntityTemplateDto>
    {
        public BoxtyEntityTemplateDto Map(BoxtyEntityTemplate entity, ClaimsPrincipal? user = null);

        public BoxtyEntityTemplate Map(BoxtyEntityTemplateDto dto, ClaimsPrincipal? user = null);

        public IEnumerable<BoxtyEntityTemplateDto> Map(IEnumerable<BoxtyEntityTemplate> entities, ClaimsPrincipal? user = null)
        {
            return entities.Select(entity => Map(entity, user));
        }

        public IEnumerable<BoxtyEntityTemplate> Map(IEnumerable<BoxtyEntityTemplateDto> dtos, ClaimsPrincipal? user = null)
        {
            return dtos.Select(dto => Map(dto, user));
        }

        public void Map(BoxtyEntityTemplateDto dto, BoxtyEntityTemplate entity, ClaimsPrincipal? user = null)
        {
            entity.Id = dto.Id;
            entity.Name = dto.Name;
            entity.Description = dto.Description;
            entity.CreatedDate = dto.CreatedDate;
            entity.CreatedBy = dto.CreatedBy;
            entity.ModifiedDate = dto.ModifiedDate;
            entity.LastModifiedBy = dto.LastModifiedBy;
        }

        public void Map(BoxtyEntityTemplate entity, BoxtyEntityTemplateDto dto, ClaimsPrincipal? user = null)
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
