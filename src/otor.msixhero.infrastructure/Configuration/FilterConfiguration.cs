using System.Runtime.Serialization;

namespace Otor.MsixHero.Infrastructure.Configuration
{
    public class FilterConfiguration : BaseJsonSetting
    {
        public FilterConfiguration()
        {
            this.ShowApps = PackageFilter.Developer;
        }

        [DataMember(Name="showApps")]
        public PackageFilter ShowApps { get; set; }

        [DataMember(Name = "addons")]
        public AddonsFilter AddonsFilter { get; set; }
    }
}