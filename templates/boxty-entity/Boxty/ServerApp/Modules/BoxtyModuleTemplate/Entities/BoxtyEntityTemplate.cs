using Boxty.ServerBase.Entities;
using Boxty.SharedBase.Interfaces;

namespace Boxty.ServerApp.Modules.BoxtyModuleTemplate.Entities
{
    public class BoxtyEntityTemplate : BaseEntity<BoxtyEntityTemplate>, IEntity
    {
        public required string Name { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
