using Boxty.ServerBase.Endpoints;
using ModuleEntity = Boxty.ServerApp.Modules.BoxtyModuleTemplate.Entities.BoxtyEntityTemplate;
using Boxty.ServerApp.Modules.BoxtyModuleTemplate.Infrastructure.Database;
using Boxty.SharedApp.DTOs.BoxtyModuleTemplate;

namespace Boxty.ServerApp.Modules.BoxtyModuleTemplate.Endpoints
{
    public class BoxtyEntityTemplateEndpoints : BaseEndpoints<ModuleEntity, BoxtyEntityTemplateDto, BoxtyModuleTemplateDbContext>, IEndpoints
    {
    }
}
