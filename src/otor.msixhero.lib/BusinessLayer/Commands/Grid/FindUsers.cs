using System.Collections.Generic;
using System.Xml.Serialization;
using otor.msixhero.lib.BusinessLayer.Models.Packages;
using otor.msixhero.lib.BusinessLayer.Models.Users;

namespace otor.msixhero.lib.BusinessLayer.Commands.Grid
{
    public class FindUsers : SelfElevatedCommand<FoundUsers>
    {
        public FindUsers()
        {
        }

        public FindUsers(Package package, bool forceElevation)
        {
            this.FullProductId = package.ProductId;
            this.ForceElevation = forceElevation;
        }

        public FindUsers(string fullProductId, bool forceElevation)
        {
            this.FullProductId = fullProductId;
            this.ForceElevation = forceElevation;
        }

        [XmlElement]
        public string FullProductId { get; set; }

        [XmlElement]
        public bool ForceElevation { get; set; }

        [XmlIgnore]
        public override bool RequiresElevation
        {
            get
            {
                return this.ForceElevation;
            }
        }
    }
}
