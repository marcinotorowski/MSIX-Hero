using System;
using System.Xml.Serialization;
using otor.msixhero.lib.Domain.Appx.Packages;

namespace otor.msixhero.lib.Domain.Commands.Packages.Developer
{
    [Serializable]
    public class MountRegistry : VoidCommand
    {
        public MountRegistry()
        {
        }

        public MountRegistry(string packageName, string installLocation, bool startRegedit = false)
        {
            this.PackageName = packageName;
            this.InstallLocation = installLocation;
            this.StartRegedit = startRegedit;
        }

        public MountRegistry(InstalledPackage package, bool startRegedit = false)
        {
            this.PackageName = package.Name;
            this.InstallLocation = package.InstallLocation;
            this.StartRegedit = startRegedit;
        }

        [XmlElement]
        public string PackageName { get; set; }

        [XmlElement]
        public string InstallLocation { get; set; }

        [XmlElement]
        public bool StartRegedit { get; set; }
    }
}
