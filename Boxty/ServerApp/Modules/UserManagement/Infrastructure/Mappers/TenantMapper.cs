using Boxty.ServerBase.Mappers;
using Boxty.ServerApp.Modules.UserManagement.Entities;
using Boxty.SharedApp.DTOs.UserManagement;
using System.Security.Claims;

namespace Boxty.ServerApp.Modules.UserManagement.Infrastructure.Mappers
{
    public class TenantMapper : IMapper<Tenant, TenantDto>
    {
        public TenantDto Map(Tenant entity, ClaimsPrincipal? user = null)
        {
            return new TenantDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Domain = entity.Domain,
                Telephone = entity.Telephone,
                Address = entity.Address,
                Postcode = entity.Postcode,
                Website = entity.Website,
                Email = entity.Email,
                Notes = entity.Notes,
                IsActive = entity.IsActive,
                CreatedBy = entity.CreatedBy,
                LastModifiedBy = entity.LastModifiedBy,
                CreatedDate = entity.CreatedDate,
                ModifiedDate = entity.ModifiedDate,
                RelatedDocumentIds = entity.RelatedDocumentIds.ToList(),
                SubjectId = entity.SubjectId,
                TenantId = entity.TenantId
            };
        }

        public Tenant Map(TenantDto dto, ClaimsPrincipal? user = null)
        {
            return new Tenant
            {
                Id = dto.Id,
                Name = dto.Name,
                Domain = dto.Domain,
                Telephone = dto.Telephone,
                Address = dto.Address,
                Postcode = dto.Postcode,
                Website = dto.Website,
                Email = dto.Email,
                Notes = dto.Notes,
                IsActive = dto.IsActive,
                RelatedDocumentIds = dto.RelatedDocumentIds.ToArray(),
                SearchTags = $"{dto.Id} {dto.Name} {dto.Telephone} {dto.Address} {dto.Postcode} {dto.Website} {dto.Email} {dto.Notes}"
            };
        }

        public void Map(TenantDto dto, Tenant entity, ClaimsPrincipal? user = null)
        {
            entity.Id = dto.Id;
            entity.Domain = dto.Domain;
            entity.Name = dto.Name;
            entity.Telephone = dto.Telephone;
            entity.Address = dto.Address;
            entity.Postcode = dto.Postcode;
            entity.Website = dto.Website;
            entity.Email = dto.Email;
            entity.Notes = dto.Notes;
            entity.IsActive = dto.IsActive;
            entity.RelatedDocumentIds = dto.RelatedDocumentIds.ToArray();
            entity.SearchTags = $"{entity.Id} {dto.Name} {dto.Telephone} {dto.Address} {dto.Postcode} {dto.Website} {dto.Email}";
        }

        public void Map(Tenant entity, TenantDto dto, ClaimsPrincipal? user = null)
        {
            dto.Id = entity.Id;
            dto.Name = entity.Name;
            dto.Domain = entity.Domain;
            dto.Telephone = entity.Telephone;
            dto.Address = entity.Address;
            dto.Postcode = entity.Postcode;
            dto.Website = entity.Website;
            dto.Email = entity.Email;
            dto.Notes = entity.Notes;
            dto.IsActive = entity.IsActive;
            dto.CreatedBy = entity.CreatedBy;
            dto.LastModifiedBy = entity.LastModifiedBy;
            dto.CreatedDate = entity.CreatedDate;
            dto.ModifiedDate = entity.ModifiedDate;
            dto.RelatedDocumentIds = entity.RelatedDocumentIds.ToList();
            dto.SubjectId = entity.SubjectId;
            dto.TenantId = entity.TenantId;
        }
    }
}
