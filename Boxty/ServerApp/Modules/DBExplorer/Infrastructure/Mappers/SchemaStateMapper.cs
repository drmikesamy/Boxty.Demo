using Boxty.ServerBase.Mappers;
using Boxty.ServerApp.Modules.DBExplorer.Entities;
using Boxty.SharedApp.DTOs.DBExplorer;
using System.Security.Claims;
using Riok.Mapperly;

namespace Boxty.ServerApp.Modules.DBExplorer.Infrastructure.Mappers
{
    [Mapper]
    public partial class SchemaStateMapper : IMapper<SchemaState, SchemaStateDto>
    {
        public partial SchemaStateDto Map(SchemaState entity, ClaimsPrincipal? user = null);
        public partial SchemaState Map(SchemaStateDto dto, ClaimsPrincipal? user = null);
        public partial void Map(SchemaStateDto dto, SchemaState entity, ClaimsPrincipal? user = null);

        public partial void Map(SchemaState entity, SchemaStateDto dto, ClaimsPrincipal? user = null);
    }
}