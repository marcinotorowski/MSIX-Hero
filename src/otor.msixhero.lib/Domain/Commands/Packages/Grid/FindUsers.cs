using System.Collections.Generic;
using System.Xml.Serialization;
using otor.msixhero.lib.Domain.Appx.Packages;
using otor.msixhero.lib.Domain.Appx.Users;

namespace otor.msixhero.lib.Domain.Commands.Packages.Grid
{
    public class FindUsers : SelfElevatedCommand<List<User>>
    {
        public FindUsers()
        {
        }

        public FindUsers(InstalledPackage package, bool forceElevation)
        {
            this.Source = package.PackageId;
            this.ForceElevation = forceElevation;
        }

        public FindUsers(string source, bool forceElevation)
        {
            this.Source = source;
            this.ForceElevation = forceElevation;
        }

        [XmlElement]
        public string Source { get; set; }

        [XmlElement]
        public bool ForceElevation { get; set; }

        [XmlIgnore]
        public override SelfElevationType RequiresElevation
        {
            get
            {
                return this.ForceElevation ? SelfElevationType.RequireAdministrator : SelfElevationType.HighestAvailable;
            }
        }
    }
}
