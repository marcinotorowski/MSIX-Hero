using System.Runtime.Serialization;

namespace otor.msixhero.lib.Infrastructure.Configuration
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
}
