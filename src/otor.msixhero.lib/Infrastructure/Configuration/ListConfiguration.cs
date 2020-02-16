using System.Collections.Generic;
using System.Runtime.Serialization;

namespace otor.msixhero.lib.Infrastructure.Configuration
{
    [DataContract]
    public class ListConfiguration : BaseJsonSetting
    {
        public ListConfiguration()
        {
            this.Sidebar = new SidebarListConfiguration();
            this.Tools = new List<ToolListConfiguration>();
            this.Filter = new FilterConfiguration();
        }

        [DataMember(Name = "filter")]
        public FilterConfiguration Filter { get; set; }

        [DataMember(Name = "sidebar")]
        public SidebarListConfiguration Sidebar { get; set; }

        [DataMember(Name = "tools")]
        public List<ToolListConfiguration> Tools { get; set; }
    }
}