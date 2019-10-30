using System;
using System.Xml.Serialization;

namespace otor.msixhero.lib.BusinessLayer.Commands.Developer
{
    [Serializable]
    public class MountRegistry : SelfElevatedCommand<bool>
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

        public MountRegistry(Package package, bool startRegedit = false)
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

        [XmlIgnore]
        public override bool RequiresElevation => true;
    }
}
