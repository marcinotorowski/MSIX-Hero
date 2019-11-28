using System.Collections.Generic;
using System.Runtime.Serialization;

namespace otor.msixhero.lib.Infrastructure.Configuration
{
    [DataContract]
    public class ListConfiguration
    {
        public ListConfiguration()
        {
            this.Sidebar = new SidebarListConfiguration();
            this.Tools = new List<ToolListConfiguration>();
        }

        [DataMember(Name = "sidebar")]
        public SidebarListConfiguration Sidebar { get; set; }

        [DataMember(Name = "tools")]
        public List<ToolListConfiguration> Tools { get; set; }
    }
}