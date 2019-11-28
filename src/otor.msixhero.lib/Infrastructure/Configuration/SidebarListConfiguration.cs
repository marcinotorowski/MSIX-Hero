using System.Runtime.Serialization;

namespace otor.msixhero.lib.Infrastructure.Configuration
{
    [DataContract]
    public class SidebarListConfiguration
    {
        [DataMember(Name = "visible")]
        public bool Visible { get; set; }
    }
}