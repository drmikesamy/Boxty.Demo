using Boxty.ServerBase.Database;
using Boxty.ServerBase.Entities;
using Microsoft.EntityFrameworkCore;
using ModuleEntity = Boxty.ServerApp.Modules.DemoModule.Entities.DemoEntity;

namespace Boxty.ServerApp.Modules.DemoModule.Infrastructure.Database
{
    public sealed class DemoModuleDbContext(DbContextOptions<DemoModuleDbContext> options)
        : BaseDbContext<DemoModuleDbContext>(options), IDbContext<DemoModuleDbContext>
    {
        public DbSet<ModuleEntity> DemoEntitys { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(Schema.DemoModule);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(DemoModuleDbContext).Assembly);
        }
    }
}
