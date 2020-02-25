using System;
using System.Xml.Serialization;
using otor.msixhero.lib.Domain.Appx.Packages;

namespace otor.msixhero.lib.Domain.Commands.Packages.Developer
{
    [Serializable]
    public class UnmountRegistry : SelfElevatedCommand
    {
        public UnmountRegistry()
        {
        }

        public UnmountRegistry(string packageName)
        {
            this.PackageName = packageName;
        }

        public UnmountRegistry(InstalledPackage package)
        {
            this.PackageName = package.Name;
        }

        [XmlElement]
        public string PackageName { get; set; }

        [XmlIgnore]
        public override SelfElevationType RequiresElevation => SelfElevationType.RequireAdministrator;
    }
}