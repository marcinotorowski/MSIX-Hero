using System;
using System.Xml.Serialization;
using otor.msixhero.lib.Domain.Appx.Packages;

namespace otor.msixhero.lib.Domain.Commands.Packages.Developer
{
    [Serializable]
    public class GetRegistryMountState : BaseCommand<RegistryMountState>
    {
        public GetRegistryMountState()
        {
        }

        public GetRegistryMountState(string installLocation, string packageName)
        {
            this.InstallLocation = installLocation;
            this.PackageName = packageName;
        }

        [XmlElement]
        public string InstallLocation { get; set; }

        [XmlElement]
        public string PackageName { get; set; }
    }
}
