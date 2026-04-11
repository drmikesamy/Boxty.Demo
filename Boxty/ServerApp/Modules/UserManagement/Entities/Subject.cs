using Boxty.ServerBase.Entities;
using Boxty.SharedBase.Interfaces;

namespace Boxty.ServerApp.Modules.UserManagement.Entities
{
    public class Subject : BaseEntity<Subject>, IEntity, ISearchTags, ISubjectEntity
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Username { get; set; }
        public required string Telephone { get; set; }
        public required string Email { get; set; }
        public Guid AvatarImageGuid { get; set; }
        public string? AvatarImage { get; set; }
        public string? AvatarTitle { get; set; }
        public string? AvatarDescription { get; set; }
        public DateTime? DOB { get; set; }
        public required string Address1 { get; set; }
        public string? Address2 { get; set; }
        public string? Address3 { get; set; }
        public required string Postcode { get; set; }
        public string Notes { get; set; } = string.Empty;
        public Guid[] RelatedDocumentIds { get; set; } = Array.Empty<Guid>();
        public string SearchTags { get; set; } = string.Empty;
        public string? RoleName { get; set; }
    }
}
