using Boxty.ServerBase.Entities;
using Boxty.SharedBase.Interfaces;

namespace Boxty.ServerApp.Modules.UserManagement.Entities
{
    public class SubjectNote : BaseEntity<SubjectNote>, IEntity
    {
        public required string Content { get; set; }
        public virtual Subject Subject { get; set; } = null!;
    }
}
