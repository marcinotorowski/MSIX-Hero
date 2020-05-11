using System;
using System.Xml.Serialization;
using otor.msixhero.lib.Domain.Appx.Manifest.Full;
using otor.msixhero.lib.Domain.Appx.Packages;

namespace otor.msixhero.lib.Domain.Commands.Packages.Grid
{
    [Serializable]
    public class GetPackageDetails : CommandWithOutput<AppxPackage>
    {
        public GetPackageDetails()
        {
        }

        public GetPackageDetails(string fullName, PackageContext context = PackageContext.CurrentUser)
        {
            this.Context = context;
            this.Source = fullName;
        }
        
        [XmlElement]
        public string Source { get; set; }
        

        [XmlElement]
        public PackageContext Context { get; set; }
}
}
