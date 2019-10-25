using System;
using System.Collections.Generic;
using System.Text;

namespace otor.msixhero.lib.BusinessLayer.Actions
{
    [Serializable]
    public class FindUsersOfPackage : BaseElevatedAction
    {
        public FindUsersOfPackage(string fullProductId)
        {
            this.FullProductId = fullProductId;
        }

        public FindUsersOfPackage()
        {
        }

        public FindUsersOfPackage(Package package) : this(package.ProductId)
        {
        }

        public string FullProductId { get; set; }

        public override bool RequiresElevation
        {
            get
            {
                return true;
            }
        }
    }
}
