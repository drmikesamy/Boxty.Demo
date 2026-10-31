using Boxty.ServerBase.Mappers;
using Boxty.ServerApp.Modules.UserManagement.Entities;
using Boxty.SharedApp.DTOs.UserManagement;
using System.Security.Claims;

namespace Boxty.ServerApp.Modules.UserManagement.Infrastructure.Mappers
{
    public class TenantNoteMapper : IMapper<TenantNote, TenantNoteDto>
    {
        public TenantNoteDto Map(TenantNote entity, ClaimsPrincipal? user = null)
        {
            return new TenantNoteDto
            {
                Id = entity.Id,
                Content = entity.Content,
                TenantId = entity.TenantId,
                CreatedDate = entity.CreatedDate,
                CreatedBy = entity.CreatedBy,
                ModifiedDate = entity.ModifiedDate,
                LastModifiedBy = entity.LastModifiedBy
            };
        }

        public TenantNote Map(TenantNoteDto dto, ClaimsPrincipal? user = null)
        {
            return new TenantNote
            {
                Id = dto.Id,
                Content = dto.Content,
                TenantId = dto.TenantId,
                CreatedDate = dto.CreatedDate,
                CreatedBy = dto.CreatedBy,
                ModifiedDate = dto.ModifiedDate,
                LastModifiedBy = dto.LastModifiedBy
            };
        }

        public void Map(TenantNoteDto dto, TenantNote entity, ClaimsPrincipal? user = null)
        {
            entity.Id = dto.Id;
            entity.Content = dto.Content;
            entity.TenantId = dto.TenantId;
            entity.CreatedDate = dto.CreatedDate;
            entity.CreatedBy = dto.CreatedBy;
            entity.ModifiedDate = dto.ModifiedDate;
            entity.LastModifiedBy = dto.LastModifiedBy;
        }

        public void Map(TenantNote entity, TenantNoteDto dto, ClaimsPrincipal? user = null)
        {
            dto.Id = entity.Id;
            dto.Content = entity.Content;
            dto.TenantId = entity.TenantId;
            dto.CreatedDate = entity.CreatedDate;
            dto.CreatedBy = entity.CreatedBy;
            dto.ModifiedDate = entity.ModifiedDate;
            dto.LastModifiedBy = entity.LastModifiedBy;
        }
    }
}
