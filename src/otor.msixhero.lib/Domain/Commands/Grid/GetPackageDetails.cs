using System;
using System.Xml.Serialization;
using otor.msixhero.lib.Domain.Appx.Manifest.Full;
using otor.msixhero.lib.Domain.Appx.Packages;

namespace otor.msixhero.lib.Domain.Commands.Grid
{
    [Serializable]
    public class GetPackageDetails : BaseCommand<AppxPackage>
    {
        public GetPackageDetails()
        {
        }

        public GetPackageDetails(string fullName)
        {
            PackageFullName = fullName;
        }

        public GetPackageDetails(Package package) : this(package.ProductId)
        {
        }

        [XmlElement]
        public string PackageFullName { get; set; }
    }
}
