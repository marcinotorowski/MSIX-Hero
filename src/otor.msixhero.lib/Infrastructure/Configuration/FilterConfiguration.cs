using System.Runtime.Serialization;
using otor.msixhero.lib.Domain.Appx.Packages;

namespace otor.msixhero.lib.Infrastructure.Configuration
{
    public class FilterConfiguration : BaseJsonSetting
    {
        public FilterConfiguration()
        {
            this.ShowApps = PackageFilter.Developer;
        }

        [DataMember(Name="showApps")]
        public PackageFilter ShowApps { get; set; }
    }
}