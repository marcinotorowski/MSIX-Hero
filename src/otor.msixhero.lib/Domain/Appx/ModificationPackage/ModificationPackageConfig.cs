using otor.msixhero.lib.Domain.Appx.AppInstaller;

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
