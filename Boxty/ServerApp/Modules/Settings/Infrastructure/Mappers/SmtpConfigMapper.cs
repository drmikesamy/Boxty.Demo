using Boxty.ServerBase.Mappers;
using Boxty.ServerApp.Modules.Settings.Entities;
using Boxty.SharedApp.DTOs.Settings;
using System.Security.Claims;
using Riok.Mapperly;

namespace Boxty.ServerApp.Modules.Settings.Infrastructure.Mappers
{
    [Mapper]
    public partial class SmtpConfigMapper : IMapper<SmtpConfig, SmtpConfigDto>
    {
        public partial SmtpConfigDto Map(SmtpConfig entity, ClaimsPrincipal? user = null);
        public partial SmtpConfig Map(SmtpConfigDto dto, ClaimsPrincipal? user = null);
    }
}