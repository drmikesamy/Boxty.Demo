using Boxty.ServerBase.Entities;

namespace Boxty.ServerApp.Modules.UserManagement.Entities
{
    public class Role : BaseEntity<Role>, IEntity
    {
        public string Name { get; set; } = string.Empty;
        public ICollection<Permission> Permissions { get; set; } = new List<Permission>();
    }
}