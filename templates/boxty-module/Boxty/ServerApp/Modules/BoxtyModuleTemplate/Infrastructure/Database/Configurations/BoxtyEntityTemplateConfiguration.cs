using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModuleEntity = Boxty.ServerApp.Modules.BoxtyModuleTemplate.Entities.BoxtyEntityTemplate;

namespace Boxty.ServerApp.Modules.BoxtyModuleTemplate.Infrastructure.Database.Configurations
{
    public class BoxtyEntityTemplateConfiguration : IEntityTypeConfiguration<ModuleEntity>
    {
        public void Configure(EntityTypeBuilder<ModuleEntity> builder)
        {
            builder.ToTable("BoxtyEntityTemplates", "BoxtyModuleTemplate");
            builder.Property(p => p.Id).HasDefaultValueSql("gen_random_uuid()");
        }
    }
}
