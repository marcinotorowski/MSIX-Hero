using System.Collections.Generic;
using otor.msixhero.lib.Domain.Appx.Packages;
using Prism.Events;

namespace otor.msixhero.lib.Domain.Events
{
    public class PackagesSelectionChangedPayLoad
    {
        public PackagesSelectionChangedPayLoad(IReadOnlyCollection<InstalledPackage> selected, IReadOnlyCollection<InstalledPackage> unselected)
        {
            Selected = selected;
            Unselected = unselected;
        }

        public IReadOnlyCollection<InstalledPackage> Selected { get; }

        public IReadOnlyCollection<InstalledPackage> Unselected { get; }
    }

    public class PackagesSelectionChanged : PubSubEvent<PackagesSelectionChangedPayLoad>
    {
    }
}
