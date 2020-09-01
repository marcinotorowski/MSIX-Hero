using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;
using Otor.MsixHero.Appx.Packaging.Installation.Enums;

namespace Otor.MsixHero.Lib.Proxy.Packaging.Dto
{

    [Serializable]
    public class GetInstalledPackagesDto : ProxyObject<List<InstalledPackage>>
    {
        public GetInstalledPackagesDto()
        {
            this.Context = PackageContext.CurrentUser;
        }

        public GetInstalledPackagesDto(PackageContext context)
        {
            this.Context = context;
        }

        [XmlElement]
        public PackageContext Context { get; set; }
    }
}
