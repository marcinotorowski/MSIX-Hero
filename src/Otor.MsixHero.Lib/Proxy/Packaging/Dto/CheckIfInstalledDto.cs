using System;
using System.Xml.Serialization;
using Otor.MsixHero.Appx.Packaging.Installation.Enums;

namespace Otor.MsixHero.Lib.Proxy.Packaging.Dto
{

    [Serializable]
    public class CheckIfInstalledDto : ProxyObject<bool>
    {
        public CheckIfInstalledDto()
        {
            this.Context = PackageContext.CurrentUser;
        }

        public CheckIfInstalledDto(PackageContext context)
        {
            this.Context = context;
        }

        [XmlElement]
        public PackageContext Context { get; set; }
        
        [XmlElement]
        public string ManifestFilePath { get; set; }
    }
}
