using Boxty.ServerBase.Mappers;
using Boxty.ServerApp.Modules.UserManagement.Entities;
using Boxty.SharedApp.DTOs.UserManagement;
using System.Security.Claims;

namespace Boxty.ServerApp.Modules.UserManagement.Infrastructure.Mappers
{
    public class TenantDocumentMapper : IMapper<TenantDocument, TenantDocumentDto>
    {
        public TenantDocumentDto Map(TenantDocument entity, ClaimsPrincipal? user = null)
        {
            return new TenantDocumentDto
            {
                Id = entity.Id,
                IsActive = entity.IsActive,
                CreatedBy = entity.CreatedBy,
                LastModifiedBy = entity.LastModifiedBy,
                CreatedDate = entity.CreatedDate,
                ModifiedDate = entity.ModifiedDate,
                Name = entity.Name,
                Description = entity.Description,
                BlobContainerName = entity.BlobContainerName,
                BlobName = entity.BlobName,
                TenantDocumentType = entity.TenantDocumentType,
                SubjectId = entity.SubjectId,
                TenantId = entity.TenantId
            };
        }

        public TenantDocument Map(TenantDocumentDto dto, ClaimsPrincipal? user = null)
        {
            return new TenantDocument
            {
                Id = dto.Id,
                IsActive = dto.IsActive,
                Name = dto.Name,
                Description = dto.Description,
                SearchTags = $"{dto.Name} {dto.Description}".ToLower(),
                TenantDocumentType = dto.TenantDocumentType,
                SubjectId = dto.SubjectId,
                TenantId = dto.TenantId
            };
        }

        public void Map(TenantDocumentDto dto, TenantDocument entity, ClaimsPrincipal? user = null)
        {
            entity.Id = dto.Id;
            entity.IsActive = dto.IsActive;
            entity.Name = dto.Name;
            entity.Description = dto.Description;
            entity.TenantDocumentType = dto.TenantDocumentType;
            entity.SubjectId = dto.SubjectId;
            entity.TenantId = dto.TenantId;
        }

        public void Map(TenantDocument entity, TenantDocumentDto dto, ClaimsPrincipal? user = null)
        {
            dto.Id = entity.Id;
            dto.IsActive = entity.IsActive;
            dto.CreatedBy = entity.CreatedBy;
            dto.LastModifiedBy = entity.LastModifiedBy;
            dto.CreatedDate = entity.CreatedDate;
            dto.ModifiedDate = entity.ModifiedDate;
            dto.Name = entity.Name;
            dto.Description = entity.Description;
            dto.BlobContainerName = entity.BlobContainerName;
            dto.BlobName = entity.BlobName;
            dto.TenantDocumentType = entity.TenantDocumentType;
            dto.SubjectId = entity.SubjectId;
            dto.TenantId = entity.TenantId;
        }
    }
}
