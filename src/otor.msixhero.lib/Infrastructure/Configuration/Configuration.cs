using System.Collections.Specialized;
using System.Runtime.Serialization;

namespace otor.msixhero.lib.Infrastructure.Configuration
{
    [DataContract(Name = "configuration")]
    public class Configuration
    {
        public Configuration()
        {
            this.List = new ListConfiguration();
            this.Signing = new SigningConfiguration();
        }

        [DataMember(Name = "list")]
        public ListConfiguration List { get; set; }

        [DataMember(Name = "signing")]
        public SigningConfiguration Signing { get; set; }
    }
}
