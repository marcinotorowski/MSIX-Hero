using otor.msixhero.lib.BusinessLayer.Models.Packages;

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
            ForceElevation = forceElevation;
        }

        public FindUsers(string fullProductId, bool forceElevation)
        {
            this.FullProductId = fullProductId;
            ForceElevation = forceElevation;
        }

        public string FullProductId { get; set; }

        public bool ForceElevation { get; set; }
    }
}
