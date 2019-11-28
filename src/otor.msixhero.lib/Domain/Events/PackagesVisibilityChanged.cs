using System.Collections.Generic;
using otor.msixhero.lib.Domain.Appx.Packages;
using Prism.Events;

namespace otor.msixhero.lib.Domain.Events
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
