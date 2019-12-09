using otor.msixhero.lib.Domain.Appx.AppInstaller;
using otor.msixhero.lib.Domain.Appx.Manifest.Full;

namespace otor.msixhero.lib.Domain.Appx.ModificationPackage
{
    public class ModificationPackageConfig
    {
        public string ParentName { get; set; }

        public string ParentPublisher { get; set; }

        public string Name { get; set; }

        public string Publisher { get; set; }

        public string Version { get; set; }

        public AppInstallerPackageArchitecture Architecture { get; set; }
    }
}
