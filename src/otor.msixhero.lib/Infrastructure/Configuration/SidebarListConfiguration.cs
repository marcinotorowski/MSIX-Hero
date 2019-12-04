using System.Runtime.Serialization;

namespace otor.msixhero.lib.Infrastructure.Configuration
{
    [DataContract]
    public class SidebarListConfiguration
    {
        public SidebarListConfiguration()
        {
            this.Visible = true;
        }

        [DataMember(Name = "visible")]
        public bool Visible { get; set; }
    }
}