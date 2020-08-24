using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using otor.msixhero.lib.Domain.Appx.Packages;

namespace otor.msixhero.lib.Domain.Commands.Packages.Grid
{
    [Serializable]
    public class GetModificationPackages : CommandWithOutput<List<InstalledPackage>>
    {
        public GetModificationPackages()
        {
            this.Context = PackageContext.CurrentUser;
        }

        public GetModificationPackages(string fullPackageName, PackageContext context = PackageContext.CurrentUser)
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