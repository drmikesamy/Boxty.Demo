using Boxty.ServerApp.Modules.Settings.Entities;
using Microsoft.EntityFrameworkCore;

namespace Boxty.ServerApp.Modules.Settings.Infrastructure.Database
{
    internal static class SettingsSeedData
    {
        private static readonly Guid SmtpConfigId = new("b3f5aa27-6b4b-4ee3-9126-09d870c4c6dd");
        private static readonly Guid RootSubjectId = new("de8d2617-58f1-4965-a523-811e2f1a1eec");
        private static readonly Guid RootTenantId = new("c1e30e05-0655-42b6-9e4f-32310eb650c8");
        private static readonly DateTime SeedDate = new(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static void Seed(SettingsDbContext dbContext)
        {
            if (dbContext.SmtpConfigs.Any())
            {
                return;
            }

            dbContext.SmtpConfigs.Add(new SmtpConfig
            {
                Id = SmtpConfigId,
                Name = "Default SMTP Configuration",
                Description = string.Empty,
                SenderAddress = "info@boxty.com",
                SenderName = "Boxty System",
                SmtpHost = "smtp.ionos.co.uk",
                SmtpPort = 465,
                SmtpUsername = "emailsmtp@boxty.com",
                SmtpPassword = "YOUR-SMTP-PASSWORD",
                EnableEmailSending = true,
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
    }
}
