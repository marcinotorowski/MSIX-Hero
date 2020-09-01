using System;
using System.Xml.Serialization;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;

namespace Otor.MsixHero.Lib.Proxy.Diagnostic.Dto
{
    [Serializable]
    public class MountRegistryDto : ProxyObject
    {
        public MountRegistryDto()
        {
        }

        public MountRegistryDto(string packageName, string installLocation, bool startRegedit = false)
        {
            this.PackageName = packageName;
            this.InstallLocation = installLocation;
            this.StartRegedit = startRegedit;
        }

        public MountRegistryDto(InstalledPackage package, bool startRegedit = false)
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
