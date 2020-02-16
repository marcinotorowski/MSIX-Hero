using System.Runtime.Serialization;

namespace otor.msixhero.lib.Infrastructure.Configuration
{
    [DataContract(Name = "packer")]
    public class PackerConfiguration : BaseJsonSetting
    {
        public PackerConfiguration()
        {
        }

        [DataMember(Name = "sign")]
        public bool SignByDefault { get; set; }
    }
}