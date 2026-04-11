using Boxty.ServerBase.Entities;

namespace Boxty.ServerApp.Modules.UserManagement.Entities
{
    public class Permission : BaseEntity<Permission>, IEntity
    {
        public string Name { get; set; } = string.Empty;
        public ICollection<Role> Roles { get; set; } = new List<Role>();
    }
}