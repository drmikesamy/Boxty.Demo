using Boxty.ServerBase.Entities;
using Boxty.SharedBase.Interfaces;

namespace Boxty.ServerApp.Modules.DemoModule.Entities
{
    public class DemoEntity : BaseEntity<DemoEntity>, IEntity
    {
        public required string Name { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
