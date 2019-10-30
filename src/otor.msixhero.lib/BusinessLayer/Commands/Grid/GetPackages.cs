using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using otor.msixhero.lib.BusinessLayer.State.Enums;

namespace otor.msixhero.lib.BusinessLayer.Commands.Grid
{

    [Serializable]
    public class GetPackages : SelfElevatedCommand<List<Package>>
    {
        public GetPackages()
        {
            this.Context = PackageContext.CurrentUser;
        }

        public GetPackages(PackageContext context)
        {
            this.Context = context;
        }

        [XmlElement]
        public PackageContext Context { get; set; }

        public override bool RequiresElevation => this.Context == PackageContext.AllUsers;
    }
}
