using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Otor.MsixHero.Infrastructure.Configuration
{
    [DataContract]
    public class PackagesConfiguration : BaseJsonSetting
    {
        public PackagesConfiguration()
        {
            this.Sidebar = new SidebarListConfiguration();
            this.Tools = new List<ToolListConfiguration>();
            this.Filter = new PackagesFilterConfiguration();
            this.Group = new PackagesGroupConfiguration();
            this.Sorting = new PackagesSortConfiguration();
        }

        [DataMember(Name = "filter")]
        public PackagesFilterConfiguration Filter { get; set; }

        [DataMember(Name = "group")]
        public PackagesGroupConfiguration Group { get; set; }

        [DataMember(Name = "sorting")]
        public PackagesSortConfiguration Sorting { get; set; }

        [DataMember(Name = "sidebar")]
        public SidebarListConfiguration Sidebar { get; set; }

        [DataMember(Name = "tools")]
        public List<ToolListConfiguration> Tools { get; set; }
    }
}