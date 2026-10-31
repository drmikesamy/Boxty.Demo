using Boxty.ServerBase.Entities;
using Boxty.SharedBase.Interfaces;

namespace Boxty.ServerApp.Modules.DBExplorer.Entities
{
    public class SchemaState : BaseEntity<SchemaState>, IEntity
    {
        public required string Name { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
