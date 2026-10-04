using Boxty.ServerBase.Database;
using Boxty.ServerBase.Entities;
using Microsoft.EntityFrameworkCore;
using Boxty.ServerApp.Modules.DBExplorer.Entities;
using Boxty.SharedApp.Enums;

namespace Boxty.ServerApp.Modules.DBExplorer.Infrastructure.Database
{
    public sealed class DBExplorerDbContext(DbContextOptions<DBExplorerDbContext> options) : BaseDbContext<DBExplorerDbContext>(options), IDbContext<DBExplorerDbContext>
    {
        // Add your DbSet properties here
        // Example: public DbSet<YourEntity> YourEntities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(Schema.DBExplorer);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(DBExplorerDbContext).Assembly);

            // Add your entity configurations here
            // Example:
            // modelBuilder.Entity<YourEntity>()
            //     .Property(p => p.Id)
            //     .HasDefaultValueSql("gen_random_uuid()");
        }
    }
}
