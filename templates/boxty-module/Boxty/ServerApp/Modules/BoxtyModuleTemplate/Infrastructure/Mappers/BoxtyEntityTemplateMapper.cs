using Boxty.ServerBase.Mappers;
using Boxty.ServerApp.Modules.BoxtyModuleTemplate.Entities;
using Boxty.SharedApp.DTOs.BoxtyModuleTemplate;
using System.Security.Claims;
using Riok.Mapperly;

namespace Boxty.ServerApp.Modules.BoxtyModuleTemplate.Infrastructure.Mappers
{
    [Mapper]
    public partial class BoxtyEntityTemplateMapper : IMapper<BoxtyEntityTemplate, BoxtyEntityTemplateDto>
    {
        public partial BoxtyEntityTemplateDto Map(BoxtyEntityTemplate entity, ClaimsPrincipal? user = null);
        public partial BoxtyEntityTemplate Map(BoxtyEntityTemplateDto dto, ClaimsPrincipal? user = null);
    }
}