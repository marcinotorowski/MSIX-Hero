using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;
using Otor.MsixHero.Appx.Packaging.Installation.Enums;
using Otor.MsixHero.Lib.Domain.Commands;

namespace Otor.MsixHero.Lib.Proxy.Packaging.Dto
{
    [Serializable]
    public class GetModificationPackagesDto : ProxyObject<List<InstalledPackage>>
    {
        public GetModificationPackagesDto()
        {
            this.Context = PackageContext.CurrentUser;
        }

        public GetModificationPackagesDto(string fullPackageName, PackageContext context = PackageContext.CurrentUser)
        {
            this.FullPackageName = fullPackageName;
            this.Context = context;
        }

        [XmlAttribute]
        public string FullPackageName { get; set; }

        [XmlElement]
        public PackageContext Context { get; set; }
    }
}