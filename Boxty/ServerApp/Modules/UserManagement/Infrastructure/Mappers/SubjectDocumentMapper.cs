using Boxty.ServerBase.Mappers;
using Boxty.ServerApp.Modules.UserManagement.Entities;
using Boxty.SharedApp.DTOs.UserManagement;
using System.Security.Claims;

namespace Boxty.ServerApp.Modules.UserManagement.Infrastructure.Mappers
{
    public class SubjectDocumentMapper : IMapper<SubjectDocument, SubjectDocumentDto>
    {
        public SubjectDocumentDto Map(SubjectDocument entity, ClaimsPrincipal? user = null)
        {
            return new SubjectDocumentDto
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
                SubjectDocumentType = entity.SubjectDocumentType,
                SubjectId = entity.SubjectId,
                TenantId = entity.TenantId
            };
        }

        public SubjectDocument Map(SubjectDocumentDto dto, ClaimsPrincipal? user = null)
        {
            return new SubjectDocument
            {
                Id = dto.Id,
                IsActive = dto.IsActive,
                CreatedBy = dto.CreatedBy,
                Name = dto.Name,
                Description = dto.Description,
                SearchTags = $"{dto.Name} {dto.Description}".ToLower(),
                SubjectDocumentType = dto.SubjectDocumentType,
                SubjectId = dto.SubjectId,
                TenantId = dto.TenantId
            };
        }

        public void Map(SubjectDocumentDto dto, SubjectDocument entity, ClaimsPrincipal? user = null)
        {
            entity.Id = dto.Id;
            entity.IsActive = dto.IsActive;
            entity.Name = dto.Name;
            entity.Description = dto.Description;
            entity.SubjectDocumentType = dto.SubjectDocumentType;
            entity.SubjectId = dto.SubjectId;
            entity.TenantId = dto.TenantId;
        }

        public void Map(SubjectDocument entity, SubjectDocumentDto dto, ClaimsPrincipal? user = null)
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
            dto.SubjectDocumentType = entity.SubjectDocumentType;
            dto.SubjectId = entity.SubjectId;
            dto.TenantId = entity.TenantId;
        }
    }
}
