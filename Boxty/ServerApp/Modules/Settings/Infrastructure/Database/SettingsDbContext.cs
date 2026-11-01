using Boxty.ServerBase.Database;
using Boxty.ServerBase.Entities;
using Microsoft.EntityFrameworkCore;
using Boxty.ServerApp.Modules.Settings.Entities;
using Boxty.SharedApp.Enums;

namespace Boxty.ServerApp.Modules.Settings.Infrastructure.Database
{
    public sealed class SettingsDbContext(DbContextOptions<SettingsDbContext> options)
        : BaseDbContext<SettingsDbContext>(options), IDbContext<SettingsDbContext>
    {
        public DbSet<SmtpConfig> SmtpConfigs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(Schema.Settings);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(SettingsDbContext).Assembly);

            modelBuilder.Entity<SmtpConfig>()
                .Property(p => p.Id)
                .HasDefaultValueSql("gen_random_uuid()");
        }
    }
}
