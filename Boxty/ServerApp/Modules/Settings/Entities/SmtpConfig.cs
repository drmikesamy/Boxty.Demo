using Boxty.ServerBase.Entities;
using Boxty.SharedBase.Interfaces;

namespace Boxty.ServerApp.Modules.Settings.Entities
{
    public class SmtpConfig : BaseEntity<SmtpConfig>, IEntity
    {
        public required string Name { get; set; }
        public string Description { get; set; } = string.Empty;
        public required string SenderAddress { get; set; }
        public required string SenderName { get; set; }
        public required string SmtpHost { get; set; }
        public required int SmtpPort { get; set; }
        public required string SmtpUsername { get; set; }
        public required string SmtpPassword { get; set; }
        public bool EnableEmailSending { get; set; }
    }
}
