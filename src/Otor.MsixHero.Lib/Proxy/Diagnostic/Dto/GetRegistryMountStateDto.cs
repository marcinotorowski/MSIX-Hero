using System;
using System.Xml.Serialization;
using Otor.MsixHero.Appx.Diagnostic.Registry.Enums;

namespace Otor.MsixHero.Lib.Proxy.Diagnostic.Dto
{
    [Serializable]
    public class GetRegistryMountStateDto : ProxyObject<RegistryMountState>
    {
        public GetRegistryMountStateDto()
        {
        }

        public GetRegistryMountStateDto(string installLocation, string packageName)
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
