using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Otor.MsixHero.Infrastructure.Configuration
{
    [DataContract]
    public class ListConfiguration : BaseJsonSetting
    {
        public ListConfiguration()
        {
            this.Sidebar = new SidebarListConfiguration();
            this.Tools = new List<ToolListConfiguration>();
            this.Filter = new FilterConfiguration();
            this.Group = new GroupConfiguration();
            this.Sorting = new SortConfiguration();
        }

        [DataMember(Name = "filter")]
        public FilterConfiguration Filter { get; set; }

        [DataMember(Name = "group")]
        public GroupConfiguration Group { get; set; }

        [DataMember(Name = "sorting")]
        public SortConfiguration Sorting { get; set; }

        [DataMember(Name = "sidebar")]
        public SidebarListConfiguration Sidebar { get; set; }

        [DataMember(Name = "tools")]
        public List<ToolListConfiguration> Tools { get; set; }
    }
}