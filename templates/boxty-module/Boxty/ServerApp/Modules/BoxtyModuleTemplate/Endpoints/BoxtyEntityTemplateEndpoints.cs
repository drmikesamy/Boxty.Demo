using Boxty.ServerBase.Endpoints;
using Boxty.ServerApp.Modules.BoxtyModuleTemplate.Entities;
using Boxty.ServerApp.Modules.BoxtyModuleTemplate.Infrastructure.Database;
using Boxty.SharedApp.DTOs.BoxtyModuleTemplate;

namespace Boxty.ServerApp.Modules.BoxtyModuleTemplate.Endpoints
{
    public class BoxtyEntityTemplateEndpoints : BaseEndpoints<BoxtyEntityTemplate, BoxtyEntityTemplateDto, BoxtyModuleTemplateDbContext>, IEndpoints
    {
    }
}
