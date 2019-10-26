using System;

namespace otor.msixhero.lib.BusinessLayer.Actions
{
    [Serializable]
    public class GetUsersOfPackage : BaseSelfElevatedBaseCommand
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
