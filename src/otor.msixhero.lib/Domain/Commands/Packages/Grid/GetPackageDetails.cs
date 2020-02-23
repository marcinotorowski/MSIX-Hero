using System;
using System.Xml.Serialization;
using otor.msixhero.lib.Domain.Appx.Manifest.Full;
using otor.msixhero.lib.Domain.Appx.Packages;

namespace otor.msixhero.lib.Domain.Commands.Packages.Grid
{
    [Serializable]
    public class GetPackageDetails : SelfElevatedCommand<AppxPackage>
    {
        public GetPackageDetails()
        {
        }

        public GetPackageDetails(string fullName, PackageContext context = PackageContext.CurrentUser)
        {
            this.Context = context;
            this.PackageFullName = fullName;
        }

        public GetPackageDetails(InstalledPackage package) : this(package.PackageId, package.Context)
        {
        }

        [XmlElement]
        public string PackageFullName { get; set; }
        

        [XmlElement]
        public PackageContext Context { get; set; }

        public override bool RequiresElevation => this.Context == PackageContext.AllUsers;
}
}
