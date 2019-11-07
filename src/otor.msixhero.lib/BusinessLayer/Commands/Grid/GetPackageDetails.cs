using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using Windows.Data.Xml.Dom;
using otor.msixhero.lib.BusinessLayer.Models.Manifest;
using otor.msixhero.lib.BusinessLayer.Models.Manifest.Full;
using otor.msixhero.lib.BusinessLayer.Models.Manifest.Summary;
using otor.msixhero.lib.BusinessLayer.Models.Packages;

namespace otor.msixhero.lib.BusinessLayer.Commands.Grid
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
