using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;

namespace otor.msixhero.lib.BusinessLayer.Models.Configuration
{
    [DataContract(Name = "configuration")]
    public class Configuration
    {
        public Configuration()
        {
            this.List = new ListConfiguration();
        }

        [DataMember(Name = "list")]
        public ListConfiguration List { get; set; }
    }

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

    [DataContract]
    public class ToolListConfiguration
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "path")]
        public string Path { get; set; }

        [DataMember(Name = "icon")]
        public string Icon { get; set; }
    }

    [DataContract]
    public class SidebarListConfiguration
    {
        [DataMember(Name = "visible")]
        public bool Visible { get; set; }
    }
}
