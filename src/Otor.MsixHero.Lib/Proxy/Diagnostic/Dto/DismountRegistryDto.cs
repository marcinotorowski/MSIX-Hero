using System;
using System.Xml.Serialization;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;

namespace Otor.MsixHero.Lib.Proxy.Diagnostic.Dto
{
    [Serializable]
    public class DismountRegistryDto : ProxyObject
    {
        public DismountRegistryDto()
        {
        }

        public DismountRegistryDto(string packageName)
        {
            this.PackageName = packageName;
        }

        public DismountRegistryDto(InstalledPackage package)
        {
            this.PackageName = package.Name;
        }

        [XmlElement]
        public string PackageName { get; set; }
    }
}