using System.Runtime.Serialization;

namespace Otor.MsixHero.Infrastructure.Configuration
{
    public class PackagesFilterConfiguration : BaseJsonSetting
    {
        public PackagesFilterConfiguration()
        {
            this.Filter = PackageFilter.Default;
        }

        [DataMember(Name="filter")]
        public PackageFilter Filter { get; set; }
    }
}