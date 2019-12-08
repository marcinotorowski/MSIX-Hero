using System.Runtime.Serialization;

namespace otor.msixhero.lib.Infrastructure.Configuration
{
    public class AppInstallerConfiguration
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