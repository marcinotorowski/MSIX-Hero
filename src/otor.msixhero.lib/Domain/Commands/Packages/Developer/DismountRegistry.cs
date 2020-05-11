using System;
using System.Xml.Serialization;
using otor.msixhero.lib.Domain.Appx.Packages;

namespace otor.msixhero.lib.Domain.Commands.Packages.Developer
{
    [Serializable]
    public class DismountRegistry : VoidCommand
    {
        public DismountRegistry()
        {
        }

        public DismountRegistry(string packageName)
        {
            this.PackageName = packageName;
        }

        public DismountRegistry(InstalledPackage package)
        {
            this.PackageName = package.Name;
        }

        [XmlElement]
        public string PackageName { get; set; }
    }
}