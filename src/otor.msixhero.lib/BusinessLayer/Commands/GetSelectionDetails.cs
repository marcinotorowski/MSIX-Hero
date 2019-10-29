using System;
using System.Collections.Generic;
using System.Text;

namespace otor.msixhero.lib.BusinessLayer.Commands
{
    public class GetSelectionDetails : BaseCommand
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
