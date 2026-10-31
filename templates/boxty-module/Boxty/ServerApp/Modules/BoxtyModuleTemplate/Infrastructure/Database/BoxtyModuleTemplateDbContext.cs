using Boxty.ServerBase.Database;
using Boxty.ServerBase.Entities;
using Microsoft.EntityFrameworkCore;
using Boxty.ServerApp.Modules.BoxtyModuleTemplate.Entities;
using Boxty.SharedApp.Enums;

namespace Boxty.ServerApp.Modules.BoxtyModuleTemplate.Infrastructure.Database
{
    public sealed class BoxtyModuleTemplateDbContext(DbContextOptions<BoxtyModuleTemplateDbContext> options)
        : BaseDbContext<BoxtyModuleTemplateDbContext>(options), IDbContext<BoxtyModuleTemplateDbContext>
    {
        public DbSet<BoxtyModuleTemplate> BoxtyModuleTemplates { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(Schema.BoxtyModuleTemplate);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(BoxtyModuleTemplateDbContext).Assembly);

            modelBuilder.Entity<BoxtyModuleTemplate>()
                .Property(p => p.Id)
                .HasDefaultValueSql("gen_random_uuid()");
        }
    }
}
