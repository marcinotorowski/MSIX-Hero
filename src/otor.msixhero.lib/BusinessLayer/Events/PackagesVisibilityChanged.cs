using System.Collections.Generic;
using otor.msixhero.lib.BusinessLayer.Models.Packages;
using Prism.Events;

namespace otor.msixhero.lib.BusinessLayer.Events
{
    public class PackagesVisibilityChangedPayLoad
    {
        public PackagesVisibilityChangedPayLoad(IReadOnlyCollection<Package> newVisible, IReadOnlyCollection<Package> newHidden)
        {
            NewVisible = newVisible;
            NewHidden = newHidden;
        }

        public IReadOnlyCollection<Package> NewVisible { get; }

        public IReadOnlyCollection<Package> NewHidden { get; }
    }

    public class PackagesVisibilityChanged : PubSubEvent<PackagesVisibilityChangedPayLoad>
    {
    }
}
