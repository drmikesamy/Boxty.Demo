using Boxty.ServerBase.Database;
using Boxty.ServerBase.Entities;
using Microsoft.EntityFrameworkCore;
using ModuleEntity = Boxty.ServerApp.Modules.BoxtyModuleTemplate.Entities.BoxtyEntityTemplate;

namespace Boxty.ServerApp.Modules.BoxtyModuleTemplate.Infrastructure.Database
{
    public sealed class BoxtyModuleTemplateDbContext(DbContextOptions<BoxtyModuleTemplateDbContext> options)
        : BaseDbContext<BoxtyModuleTemplateDbContext>(options), IDbContext<BoxtyModuleTemplateDbContext>
    {
        public DbSet<ModuleEntity> BoxtyEntityTemplates { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(Schema.BoxtyModuleTemplate);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(BoxtyModuleTemplateDbContext).Assembly);
        }
    }
}
