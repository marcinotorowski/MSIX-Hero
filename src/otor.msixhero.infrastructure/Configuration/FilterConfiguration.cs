using System.Runtime.Serialization;

namespace Otor.MsixHero.Infrastructure.Configuration
{
    public class FilterConfiguration : BaseJsonSetting
    {
        public FilterConfiguration()
        {
            this.PackageFilter = PackageFilter.Default;
        }

        [DataMember(Name="packageFilter")]
        public PackageFilter PackageFilter { get; set; }
    }
}