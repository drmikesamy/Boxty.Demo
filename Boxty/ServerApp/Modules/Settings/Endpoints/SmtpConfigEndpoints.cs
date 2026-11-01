using Boxty.ServerBase.Endpoints;
using Boxty.ServerApp.Modules.Settings.Entities;
using Boxty.ServerApp.Modules.Settings.Infrastructure.Database;
using Boxty.SharedApp.DTOs.Settings;

namespace Boxty.ServerApp.Modules.Settings.Endpoints
{
    public class SmtpConfigEndpoints : BaseEndpoints<SmtpConfig, SmtpConfigDto, SettingsDbContext>, IEndpoints
    {
    }
}
