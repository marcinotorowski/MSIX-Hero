using System.Runtime.Serialization;

namespace otor.msixhero.lib.Infrastructure.Configuration
{
    [DataContract]
    public class UpdateConfiguration
    {
        [DataMember(Name = "lastShownVersion")]
        public string LastShownVersion { get; set; }

        [DataMember(Name = "hideNewVersionInfo")]
        public bool HideNewVersionInfo { get; set; }
    }
}
