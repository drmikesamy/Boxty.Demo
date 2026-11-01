using Boxty.SharedBase.DTOs;
using Boxty.SharedBase.Interfaces;
using Boxty.SharedApp.DTOs;

namespace Boxty.SharedApp.DTOs.Settings
{
    public class SmtpConfigDto : BaseDto<SmtpConfigDto>, IDto, IAuditDto, IAutoCrud
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

        public string DisplayName => "TODO: Set display name"; // TODO: Update this
    }
}
