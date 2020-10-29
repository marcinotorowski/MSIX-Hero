using System.Runtime.Serialization;

namespace Otor.MsixHero.Infrastructure.Configuration
{
    public class DeviceGuardConfiguration : BaseJsonSetting
    {
        public DeviceGuardConfiguration()
        {
        }
        
        [DataMember(Name = "accessToken")]
        public string EncodedAccessToken { get; set; }

        [DataMember(Name = "refreshToken")]
        public string EncodedRefreshToken { get; set; }

        [DataMember(Name = "useV1")]
        public bool UseV1 { get; set; }

        [DataMember(Name = "subject")]
        public string Subject { get; set; }
    }
}
