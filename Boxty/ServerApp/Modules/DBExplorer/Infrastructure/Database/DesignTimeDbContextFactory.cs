using Boxty.ServerBase.Database;
using Microsoft.EntityFrameworkCore;

namespace Boxty.ServerApp.Modules.DBExplorer.Infrastructure.Database
{
    public class DesignTimeDBExplorerDbContextFactory : DesignTimeDbContextFactory<DBExplorerDbContext>
    {
    }
}
