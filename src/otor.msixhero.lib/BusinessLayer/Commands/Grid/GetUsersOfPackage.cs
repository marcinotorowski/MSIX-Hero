using System;
using System.Collections.Generic;
using otor.msixhero.lib.BusinessLayer.Models;
using otor.msixhero.lib.BusinessLayer.Models.Packages;
using otor.msixhero.lib.BusinessLayer.Models.Users;

namespace otor.msixhero.lib.BusinessLayer.Commands.Grid
{
    [Serializable]
    public class GetUsersOfPackage : SelfElevatedCommand<List<User>>
    {
        public GetUsersOfPackage(string fullProductId)
        {
            this.FullProductId = fullProductId;
        }

        public GetUsersOfPackage()
        {
        }

        public GetUsersOfPackage(Package package) : this(package.ProductId)
        {
        }

        public string FullProductId { get; set; }

        public override bool RequiresElevation => true;
    }
}
