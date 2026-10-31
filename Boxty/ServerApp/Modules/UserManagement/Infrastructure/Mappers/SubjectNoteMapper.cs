using Boxty.ServerBase.Mappers;
using Boxty.ServerApp.Modules.UserManagement.Entities;
using Boxty.SharedApp.DTOs.UserManagement;
using System.Security.Claims;

namespace Boxty.ServerApp.Modules.UserManagement.Infrastructure.Mappers
{
    public class SubjectNoteMapper : IMapper<SubjectNote, SubjectNoteDto>
    {
        public SubjectNoteDto Map(SubjectNote entity, ClaimsPrincipal? user = null)
        {
            return new SubjectNoteDto
            {
                Id = entity.Id,
                Content = entity.Content,
                SubjectId = entity.SubjectId,
                TenantId = entity.TenantId,
                CreatedDate = entity.CreatedDate,
                CreatedBy = entity.CreatedBy,
                ModifiedDate = entity.ModifiedDate,
                LastModifiedBy = entity.LastModifiedBy
            };
        }

        public SubjectNote Map(SubjectNoteDto dto, ClaimsPrincipal? user = null)
        {
            return new SubjectNote
            {
                Id = dto.Id,
                Content = dto.Content,
                SubjectId = dto.SubjectId,
                TenantId = dto.TenantId,
                CreatedDate = dto.CreatedDate,
                CreatedBy = dto.CreatedBy,
                ModifiedDate = dto.ModifiedDate,
                LastModifiedBy = dto.LastModifiedBy
            };
        }

        public void Map(SubjectNoteDto dto, SubjectNote entity, ClaimsPrincipal? user = null)
        {
            entity.Id = dto.Id;
            entity.Content = dto.Content;
            entity.SubjectId = dto.SubjectId;
            entity.TenantId = dto.TenantId;
            entity.CreatedDate = dto.CreatedDate;
            entity.CreatedBy = dto.CreatedBy;
            entity.ModifiedDate = dto.ModifiedDate;
            entity.LastModifiedBy = dto.LastModifiedBy;
        }

        public void Map(SubjectNote entity, SubjectNoteDto dto, ClaimsPrincipal? user = null)
        {
            dto.Id = entity.Id;
            dto.Content = entity.Content;
            dto.SubjectId = entity.SubjectId;
            dto.TenantId = entity.TenantId;
            dto.CreatedDate = entity.CreatedDate;
            dto.CreatedBy = entity.CreatedBy;
            dto.ModifiedDate = entity.ModifiedDate;
            dto.LastModifiedBy = entity.LastModifiedBy;
        }
    }
}
