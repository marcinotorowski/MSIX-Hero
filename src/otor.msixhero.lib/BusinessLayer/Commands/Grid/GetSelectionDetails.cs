using otor.msixhero.lib.BusinessLayer.Models;
using otor.msixhero.lib.BusinessLayer.Models.Packages;

namespace otor.msixhero.lib.BusinessLayer.Commands.Grid
{
    public class GetSelectionDetails : SelfElevatedCommand<SelectionDetails>
    {
        public GetSelectionDetails()
        {
        }

        public GetSelectionDetails(Package package, bool forceElevation)
        {
            this.FullProductId = package.ProductId;
            ForceElevation = forceElevation;
        }

        public GetSelectionDetails(string fullProductId, bool forceElevation)
        {
            this.FullProductId = fullProductId;
            ForceElevation = forceElevation;
        }

        public string FullProductId { get; set; }

        public bool ForceElevation { get; set; }
    }
}
