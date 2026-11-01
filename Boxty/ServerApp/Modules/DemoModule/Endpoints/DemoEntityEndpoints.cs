using Boxty.ServerBase.Endpoints;
using ModuleEntity = Boxty.ServerApp.Modules.DemoModule.Entities.DemoEntity;
using Boxty.ServerApp.Modules.DemoModule.Infrastructure.Database;
using Boxty.SharedApp.DTOs.DemoModule;

namespace Boxty.ServerApp.Modules.DemoModule.Endpoints
{
    public class DemoEntityEndpoints : BaseEndpoints<ModuleEntity, DemoEntityDto, DemoModuleDbContext>, IEndpoints
    {
    }
}
