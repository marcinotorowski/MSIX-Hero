using System.Runtime.Serialization;

namespace Otor.MsixHero.Infrastructure.Configuration
{
    [DataContract]
    public class AdvancedConfiguration : BaseJsonSetting
    {
        [DataMember(Name = "disableMultiThreadingForGetPackages")]
        public bool DisableMultiThreadingForGetPackages { get; set; }

        [DataMember(Name = "maxThreadsForGetPackages")]
        public int? MaxThreadsForGetPackages { get; set; }
    }
}
