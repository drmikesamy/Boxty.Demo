using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModuleEntity = Boxty.ServerApp.Modules.DemoModule.Entities.DemoEntity;

namespace Boxty.ServerApp.Modules.DemoModule.Infrastructure.Database.Configurations
{
    public class DemoEntityConfiguration : IEntityTypeConfiguration<ModuleEntity>
    {
        public void Configure(EntityTypeBuilder<ModuleEntity> builder)
        {
            builder.ToTable("DemoEntitys", "DemoModule");
            builder.Property(p => p.Id).HasDefaultValueSql("gen_random_uuid()");
        }
    }
}
