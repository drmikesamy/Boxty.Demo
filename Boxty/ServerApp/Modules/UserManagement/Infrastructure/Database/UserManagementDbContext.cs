using Boxty.ServerBase.Database;
using Microsoft.EntityFrameworkCore;
using Boxty.ServerApp.Modules.UserManagement.Entities;

namespace Boxty.ServerApp.Modules.UserManagement.Infrastructure.Database
{
    public sealed class UserManagementDbContext(DbContextOptions<UserManagementDbContext> options) : BaseDbContext<UserManagementDbContext>(options), IDbContext<UserManagementDbContext>
    {
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<TenantDocument> TenantDocuments { get; set; }
        public DbSet<SubjectDocument> SubjectDocuments { get; set; }
        public DbSet<TenantNote> TenantNotes { get; set; }
        public DbSet<SubjectNote> SubjectNotes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(Schema.UserManagement);
            modelBuilder.Entity<Role>().Navigation(role => role.Permissions).AutoInclude();
        }
    }
}
