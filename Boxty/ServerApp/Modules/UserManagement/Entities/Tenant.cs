using Boxty.ServerBase.Auth;
using Boxty.ServerBase.Entities;
using Boxty.SharedBase.Interfaces;


namespace Boxty.ServerApp.Modules.UserManagement.Entities
{
    public class Tenant : BaseEntity<Tenant>, IEntity, ISearchTags, ITenantEntity
    {
        public required string Name { get; set; }
        public required string Domain { get; set; }
        public required string Telephone { get; set; }
        public required string Address { get; set; }
        public required string Postcode { get; set; }
        public required string Website { get; set; }
        public required string Email { get; set; }
        public string Notes { get; set; } = string.Empty;
        public HashSet<Subject> Subjects { get; set; } = new();
        public Guid[] RelatedDocumentIds { get; set; } = Array.Empty<Guid>();
        public string SearchTags { get; set; } = string.Empty;
    }
}
