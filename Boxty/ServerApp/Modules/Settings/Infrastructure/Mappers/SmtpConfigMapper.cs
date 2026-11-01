using Boxty.ServerBase.Mappers;
using Boxty.ServerApp.Modules.Settings.Entities;
using Boxty.SharedApp.DTOs.Settings;
using System.Security.Claims;

namespace Boxty.ServerApp.Modules.Settings.Infrastructure.Mappers
{
    public class SmtpConfigMapper : IMapper<SmtpConfig, SmtpConfigDto>
    {
        public SmtpConfigDto Map(SmtpConfig entity, ClaimsPrincipal? user = null)
        {
            return new SmtpConfigDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                SenderAddress = entity.SenderAddress,
                SenderName = entity.SenderName,
                SmtpHost = entity.SmtpHost,
                SmtpPort = entity.SmtpPort,
                SmtpUsername = entity.SmtpUsername,
                SmtpPassword = entity.SmtpPassword,
                EnableEmailSending = entity.EnableEmailSending,
                CreatedDate = entity.CreatedDate,
                CreatedBy = entity.CreatedBy,
                ModifiedDate = entity.ModifiedDate,
                LastModifiedBy = entity.LastModifiedBy

            };
        }

        public SmtpConfig Map(SmtpConfigDto dto, ClaimsPrincipal? user = null)
        {
            return new SmtpConfig
            {
                Id = dto.Id,
                Name = dto.Name,
                Description = dto.Description,
                SenderAddress = dto.SenderAddress,
                SenderName = dto.SenderName,
                SmtpHost = dto.SmtpHost,
                SmtpPort = dto.SmtpPort,
                SmtpUsername = dto.SmtpUsername,
                SmtpPassword = dto.SmtpPassword,
                EnableEmailSending = dto.EnableEmailSending,
                CreatedDate = dto.CreatedDate,
                CreatedBy = dto.CreatedBy,
                ModifiedDate = dto.ModifiedDate,
                LastModifiedBy = dto.LastModifiedBy

            };
        }

        public void Map(SmtpConfigDto dto, SmtpConfig entity, ClaimsPrincipal? user = null)
        {
            entity.Id = dto.Id;
            entity.Name = dto.Name;
            entity.Description = dto.Description;
            entity.SenderAddress = dto.SenderAddress;
            entity.SenderName = dto.SenderName;
            entity.SmtpHost = dto.SmtpHost;
            entity.SmtpPort = dto.SmtpPort;
            entity.SmtpUsername = dto.SmtpUsername;
            entity.SmtpPassword = dto.SmtpPassword;
            entity.EnableEmailSending = dto.EnableEmailSending;
            entity.CreatedDate = dto.CreatedDate;
            entity.CreatedBy = dto.CreatedBy;
            entity.ModifiedDate = dto.ModifiedDate;
            entity.LastModifiedBy = dto.LastModifiedBy;
        }

        public void Map(SmtpConfig entity, SmtpConfigDto dto, ClaimsPrincipal? user = null)
        {
            dto.Id = entity.Id;
            dto.Name = entity.Name;
            dto.Description = entity.Description;
            dto.SenderAddress = entity.SenderAddress;
            dto.SenderName = entity.SenderName;
            dto.SmtpHost = entity.SmtpHost;
            dto.SmtpPort = entity.SmtpPort;
            dto.SmtpUsername = entity.SmtpUsername;
            dto.SmtpPassword = entity.SmtpPassword;
            dto.EnableEmailSending = entity.EnableEmailSending;
            dto.CreatedDate = entity.CreatedDate;
            dto.CreatedBy = entity.CreatedBy;
            dto.ModifiedDate = entity.ModifiedDate;
            dto.LastModifiedBy = entity.LastModifiedBy;
        }
    }
}
