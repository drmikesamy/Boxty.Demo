using Boxty.ServerBase.Mappers;
using Boxty.ServerApp.Modules.UserManagement.Entities;
using Boxty.SharedApp.DTOs.UserManagement;
using System.Security.Claims;

namespace Boxty.ServerApp.Modules.UserManagement.Infrastructure.Mappers
{
    public class SubjectMapper : IMapper<Subject, SubjectDto>
    {
        public SubjectDto Map(Subject entity, ClaimsPrincipal? user = null)
        {
            return new SubjectDto
            {
                Id = entity.Id,
                IsActive = entity.IsActive,
                CreatedBy = entity.CreatedBy,
                LastModifiedBy = entity.LastModifiedBy,
                CreatedDate = entity.CreatedDate,
                ModifiedDate = entity.ModifiedDate,
                FirstName = entity.FirstName,
                LastName = entity.LastName,
                Username = entity.Username,
                DOB = entity.DOB,
                Address1 = entity.Address1,
                Address2 = entity.Address2,
                Address3 = entity.Address3,
                Postcode = entity.Postcode,
                Email = entity.Email,
                Telephone = entity.Telephone,
                AvatarImageGuid = entity.AvatarImageGuid,
                AvatarImage = entity.AvatarImage ?? string.Empty,
                AvatarTitle = string.IsNullOrEmpty(entity.AvatarTitle) ? $"{entity.FirstName} {entity.LastName}" : entity.AvatarTitle,
                Notes = entity.Notes,
                TenantId = entity.TenantId,
                RelatedDocumentIds = entity.RelatedDocumentIds.ToList(),
                RoleName = entity.RoleName,
                SubjectId = entity.SubjectId,
            };
        }

        public Subject Map(SubjectDto dto, ClaimsPrincipal? user = null)
        {
            return new Subject
            {
                Id = dto.Id,
                IsActive = dto.IsActive,
                CreatedBy = dto.CreatedBy,
                LastModifiedBy = dto.LastModifiedBy,
                CreatedDate = dto.CreatedDate,
                ModifiedDate = dto.ModifiedDate,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Username = dto.Email,
                DOB = dto.DOB,
                Address1 = dto.Address1,
                Address2 = dto.Address2,
                Address3 = dto.Address3,
                Postcode = dto.Postcode,
                Email = dto.Email,
                Telephone = dto.Telephone,
                AvatarImageGuid = dto.AvatarImageGuid,
                AvatarImage = dto.AvatarImage,
                AvatarTitle = string.IsNullOrEmpty(dto.AvatarTitle) ? $"{dto.FirstName} {dto.LastName}" : dto.AvatarTitle,
                Notes = dto.Notes,
                TenantId = dto.TenantId,
                RelatedDocumentIds = dto.RelatedDocumentIds.ToArray(),
                SearchTags = $"{dto.Id} {dto.FirstName} {dto.LastName} {dto.Telephone} {dto.Email}",
                RoleName = dto.RoleName
            };
        }

        public void Map(SubjectDto dto, Subject entity, ClaimsPrincipal? user = null)
        {
            entity.Id = dto.Id;
            entity.IsActive = dto.IsActive;
            entity.FirstName = dto.FirstName;
            entity.LastName = dto.LastName;
            entity.Username = dto.Email;
            entity.DOB = dto.DOB;
            entity.Address1 = dto.Address1;
            entity.Address2 = dto.Address2;
            entity.Address3 = dto.Address3;
            entity.Postcode = dto.Postcode;
            entity.Email = dto.Email;
            entity.Telephone = dto.Telephone;
            entity.AvatarImageGuid = dto.AvatarImageGuid;
            entity.AvatarImage = dto.AvatarImage;
            entity.AvatarTitle = string.IsNullOrEmpty(dto.AvatarTitle) ? $"{dto.FirstName} {dto.LastName}" : dto.AvatarTitle;
            entity.Notes = dto.Notes;
            entity.TenantId = dto.TenantId;
            entity.SubjectId = dto.SubjectId;
            entity.RelatedDocumentIds = dto.RelatedDocumentIds.ToArray();
            entity.SearchTags = $"{entity.Id} {dto.FirstName} {dto.LastName} {dto.Telephone} {dto.Email}";
            entity.RoleName = dto.RoleName;
        }

        public void Map(Subject entity, SubjectDto dto, ClaimsPrincipal? user = null)
        {
            dto.Id = entity.Id;
            dto.IsActive = entity.IsActive;
            dto.CreatedBy = entity.CreatedBy;
            dto.LastModifiedBy = entity.LastModifiedBy;
            dto.CreatedDate = entity.CreatedDate;
            dto.ModifiedDate = entity.ModifiedDate;
            dto.FirstName = entity.FirstName;
            dto.LastName = entity.LastName;
            dto.Username = entity.Username;
            dto.DOB = entity.DOB;
            dto.Address1 = entity.Address1;
            dto.Address2 = entity.Address2;
            dto.Address3 = entity.Address3;
            dto.Postcode = entity.Postcode;
            dto.Email = entity.Email;
            dto.Telephone = entity.Telephone;
            dto.AvatarImageGuid = entity.AvatarImageGuid;
            dto.AvatarImage = entity.AvatarImage ?? string.Empty;
            dto.AvatarTitle = string.IsNullOrEmpty(entity.AvatarTitle) ? $"{entity.FirstName} {entity.LastName}" : entity.AvatarTitle;
            dto.Notes = entity.Notes;
            dto.TenantId = entity.TenantId;
            dto.SubjectId = entity.SubjectId;
            dto.RelatedDocumentIds = entity.RelatedDocumentIds.ToList();
            dto.RoleName = entity.RoleName;
        }
    }
}
