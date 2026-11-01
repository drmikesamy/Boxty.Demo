using Boxty.SharedBase.DTOs;
using Boxty.SharedBase.Interfaces;
using Boxty.SharedApp.DTOs;

namespace Boxty.SharedApp.DTOs.DemoModule
{
    public class DemoEntityDto : BaseDto<DemoEntityDto>, IDto, IAuditDto, IAutoCrud
    {
        public required string Name { get; set; }
        public string Description { get; set; } = string.Empty;

        public string DisplayName => Name;
    }
}
