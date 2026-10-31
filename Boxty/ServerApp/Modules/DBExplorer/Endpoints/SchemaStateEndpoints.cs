using Boxty.ServerBase.Endpoints;
using Boxty.ServerApp.Modules.DBExplorer.Entities;
using Boxty.ServerApp.Modules.DBExplorer.Infrastructure.Database;
using Boxty.SharedApp.DTOs.DBExplorer;

namespace Boxty.ServerApp.Modules.DBExplorer.Endpoints
{
    public class SchemaStateEndpoints : BaseEndpoints<SchemaState, SchemaStateDto, DBExplorerDbContext>, IEndpoints
    {
    }
}
