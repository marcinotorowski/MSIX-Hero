using System.Collections.Generic;
using System.Xml.Serialization;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;
using Otor.MsixHero.Appx.Users;

namespace Otor.MsixHero.Lib.Proxy.Packaging.Dto
{
    public class GetUsersForPackageDto : ProxyObject<List<User>>
    {
        public GetUsersForPackageDto()
        {
        }

        public GetUsersForPackageDto(InstalledPackage package)
        {
            this.Source = package.PackageId;
        }

        public GetUsersForPackageDto(string source)
        {
            this.Source = source;
        }

        [XmlElement]
        public string Source { get; set; }
    }
}
