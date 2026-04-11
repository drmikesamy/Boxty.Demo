using Boxty.ServerBase.Entities;
using Boxty.SharedBase.Interfaces;

namespace Boxty.ServerApp.Modules.UserManagement.Entities
{
    public class TenantNote : BaseEntity<TenantNote>, IEntity
    {
        public required string Content { get; set; }
        public virtual Tenant Tenant { get; set; } = null!;
    }
}
