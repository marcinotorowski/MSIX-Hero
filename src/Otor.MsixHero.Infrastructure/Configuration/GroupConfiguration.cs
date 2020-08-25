using System.Runtime.Serialization;

namespace Otor.MsixHero.Infrastructure.Configuration
{
    public class GroupConfiguration : BaseJsonSetting
    {
        public GroupConfiguration()
        {
            this.GroupMode = PackageGroup.None;
        }

        [DataMember(Name= "groupMode")]
        public PackageGroup GroupMode { get; set; }
    }
}