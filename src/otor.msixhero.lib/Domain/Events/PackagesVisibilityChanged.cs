using System.Collections.Generic;
using otor.msixhero.lib.Domain.Appx.Packages;
using Prism.Events;

namespace otor.msixhero.lib.Domain.Events
{
    public class PackagesVisibilityChangedPayLoad
    {
        public PackagesVisibilityChangedPayLoad(IReadOnlyCollection<InstalledPackage> newVisible, IReadOnlyCollection<InstalledPackage> newHidden)
        {
            NewVisible = newVisible;
            NewHidden = newHidden;
        }

        public IReadOnlyCollection<InstalledPackage> NewVisible { get; }

        public IReadOnlyCollection<InstalledPackage> NewHidden { get; }
    }

    public class PackagesVisibilityChanged : PubSubEvent<PackagesVisibilityChangedPayLoad>
    {
    }
}
