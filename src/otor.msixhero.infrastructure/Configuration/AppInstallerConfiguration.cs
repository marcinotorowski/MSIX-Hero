using System.Runtime.Serialization;

namespace Otor.MsixHero.Infrastructure.Configuration
{
    public class AppInstallerConfiguration : BaseJsonSetting
    {
        public AppInstallerConfiguration()
        {
            this.DefaultRemoteLocationPackages = "http://server-name";
            this.DefaultRemoteLocationAppInstaller = "http://server-name";
        }

        [DataMember(Name = "defaultRemoteLocationPackages")]
        public string DefaultRemoteLocationPackages { get; set; }


        [DataMember(Name = "defaultRemoteLocationAppInstaller")]
        public string DefaultRemoteLocationAppInstaller { get; set; }
    }
}