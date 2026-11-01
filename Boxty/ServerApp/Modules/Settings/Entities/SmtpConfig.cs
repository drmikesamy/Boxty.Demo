using Boxty.ServerBase.Entities;
using Boxty.SharedBase.Interfaces;

namespace Boxty.ServerApp.Modules.Settings.Entities
{
    public class SmtpConfig : BaseEntity<SmtpConfig>, IEntity
    {
        public required string Name { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
