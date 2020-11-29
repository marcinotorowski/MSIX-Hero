using System.Runtime.Serialization;

namespace Otor.MsixHero.Infrastructure.Configuration
{
    public class PackagesGroupConfiguration : BaseJsonSetting
    {
        public PackagesGroupConfiguration()
        {
            this.GroupMode = PackageGroup.None;
        }

        [DataMember(Name= "groupMode")]
        public PackageGroup GroupMode { get; set; }
    }
}