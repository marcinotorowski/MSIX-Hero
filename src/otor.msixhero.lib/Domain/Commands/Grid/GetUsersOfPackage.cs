using System;
using System.Collections.Generic;
using otor.msixhero.lib.Domain.Appx.Packages;
using otor.msixhero.lib.Domain.Appx.Users;

namespace otor.msixhero.lib.Domain.Commands.Grid
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

        public GetUsersOfPackage(InstalledPackage package) : this(package.PackageId)
        {
        }

        public string FullProductId { get; set; }

        public override bool RequiresElevation => true;
    }
}
