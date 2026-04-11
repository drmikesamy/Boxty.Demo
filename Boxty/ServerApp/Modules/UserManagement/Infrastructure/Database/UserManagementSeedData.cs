using System.Security.Cryptography;
using System.Text;
using Boxty.ServerApp.Modules.UserManagement.Entities;
using Boxty.ServerApp.Modules.UserManagement.Infrastructure.Database.Enums;
using Boxty.ServerBase.Entities;
using Microsoft.EntityFrameworkCore;

namespace Boxty.ServerApp.Modules.UserManagement.Infrastructure.Database
{
    internal static class UserManagementSeedData
    {
        private static readonly Guid RootSubjectId = new("de8d2617-58f1-4965-a523-811e2f1a1eec");
        private static readonly Guid RootTenantId = new("c1e30e05-0655-42b6-9e4f-32310eb650c8");
        private static readonly Guid AdministratorRoleId = new("6a27cb2b-6a19-43b0-bef1-1d809f9e1c40");
        private static readonly DateTime SeedDate = new(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static void Seed(UserManagementDbContext dbContext)
        {
            EnsureRootTenant(dbContext);
            EnsureRootSubject(dbContext);
            EnsureAdministratorRole(dbContext);
            EnsurePermissions(dbContext);
        }

        private static void EnsureRootTenant(UserManagementDbContext dbContext)
        {
            if (dbContext.Tenants.Any(entity => entity.Id == RootTenantId))
            {
                return;
            }

            dbContext.Tenants.Add(new Tenant
            {
                Id = RootTenantId,
                Name = "Boxty",
                Domain = "boxty.org",
                Telephone = string.Empty,
                Address = string.Empty,
                Postcode = string.Empty,
                Website = "https://boxty.org",
                Email = "admin@boxty.org",
                Notes = "Root tenant organization",
                RelatedDocumentIds = Array.Empty<Guid>(),
                SearchTags = string.Empty,
                IsActive = true,
                CreatedBy = "System",
                LastModifiedBy = "System",
                CreatedDate = SeedDate,
                ModifiedDate = SeedDate,
                TenantId = RootTenantId,
                SubjectId = RootSubjectId,
                CreatedById = RootSubjectId,
                ModifiedById = RootSubjectId
            });

            dbContext.SaveChanges();
        }

        private static void EnsureRootSubject(UserManagementDbContext dbContext)
        {
            if (dbContext.Subjects.Any(entity => entity.Id == RootSubjectId))
            {
                return;
            }

            dbContext.Subjects.Add(new Subject
            {
                Id = RootSubjectId,
                FirstName = "Admin",
                LastName = "User",
                Username = "admin",
                Telephone = string.Empty,
                Email = "admin@boxty.org",
                AvatarImageGuid = Guid.Empty,
                Address1 = string.Empty,
                Postcode = string.Empty,
                Notes = "Root administrator",
                RelatedDocumentIds = Array.Empty<Guid>(),
                SearchTags = string.Empty,
                RoleName = "Administrator",
                IsActive = true,
                CreatedBy = "System",
                LastModifiedBy = "System",
                CreatedDate = SeedDate,
                ModifiedDate = SeedDate,
                TenantId = RootTenantId,
                SubjectId = RootSubjectId,
                CreatedById = RootSubjectId,
                ModifiedById = RootSubjectId
            });

            dbContext.SaveChanges();
        }

        private static void EnsureAdministratorRole(UserManagementDbContext dbContext)
        {
            if (dbContext.Roles.Any(entity => entity.Id == AdministratorRoleId))
            {
                return;
            }

            dbContext.Roles.Add(new Role
            {
                Id = AdministratorRoleId,
                Name = "Administrator",
                IsActive = true,
                CreatedBy = "System",
                LastModifiedBy = "System",
                CreatedDate = SeedDate,
                ModifiedDate = SeedDate,
                TenantId = RootTenantId,
                SubjectId = RootSubjectId,
                CreatedById = RootSubjectId,
                ModifiedById = RootSubjectId
            });

            dbContext.SaveChanges();
        }

        private static void EnsurePermissions(UserManagementDbContext dbContext)
        {
            var permissionSeeds = BuildPermissionSeeds();
            var permissionIds = permissionSeeds.Select(permission => permission.Id).ToList();
            var existingPermissionIds = dbContext.Permissions
                .Where(permission => permissionIds.Contains(permission.Id))
                .Select(permission => permission.Id)
                .ToHashSet();

            foreach (var permissionSeed in permissionSeeds.Where(permission => !existingPermissionIds.Contains(permission.Id)))
            {
                dbContext.Permissions.Add(permissionSeed);
            }

            dbContext.SaveChanges();

            var administratorRole = dbContext.Roles
                .Include(role => role.Permissions)
                .Single(role => role.Id == AdministratorRoleId);
            var permissions = dbContext.Permissions
                .Where(permission => permissionIds.Contains(permission.Id))
                .ToList();

            foreach (var permission in permissions)
            {
                if (administratorRole.Permissions.All(existing => existing.Id != permission.Id))
                {
                    administratorRole.Permissions.Add(permission);
                }
            }

            dbContext.SaveChanges();
        }

        private static IReadOnlyList<Permission> BuildPermissionSeeds()
        {
            var entityTypes = typeof(Role).Assembly.GetTypes()
                .Where(type => type.IsClass && !type.IsAbstract)
                .Where(type => type.Namespace == typeof(Role).Namespace)
                .Where(type => typeof(IEntity).IsAssignableFrom(type))
                .OrderBy(type => type.Name)
                .ToList();

            var permissions = new List<Permission>();

            foreach (var entityType in entityTypes)
            {
                foreach (var operation in Enum.GetValues<PermissionEnum>())
                {
                    var permissionName = $"{operation}{entityType.Name}";

                    permissions.Add(new Permission
                    {
                        Id = CreateDeterministicGuid($"permission:{permissionName}"),
                        Name = permissionName,
                        IsActive = true,
                        CreatedBy = "System",
                        LastModifiedBy = "System",
                        CreatedDate = SeedDate,
                        ModifiedDate = SeedDate,
                        TenantId = RootTenantId,
                        SubjectId = RootSubjectId,
                        CreatedById = RootSubjectId,
                        ModifiedById = RootSubjectId
                    });
                }
            }

            return permissions;
        }

        private static Guid CreateDeterministicGuid(string value)
        {
            var hash = MD5.HashData(Encoding.UTF8.GetBytes(value));
            return new Guid(hash);
        }
    }
}