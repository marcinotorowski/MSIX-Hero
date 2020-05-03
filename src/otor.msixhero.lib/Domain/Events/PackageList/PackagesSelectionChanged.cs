using System.Collections.Generic;
using otor.msixhero.lib.Domain.Appx.Packages;
using Prism.Events;

namespace otor.msixhero.lib.Domain.Events.PackageList
{
    public class PackagesSelectionChangedPayLoad
    {
        public PackagesSelectionChangedPayLoad(IReadOnlyCollection<InstalledPackage> selected, IReadOnlyCollection<InstalledPackage> unselected, bool isExplicit)
        {
            this.Selected = selected;
            this.Unselected = unselected;
            this.IsExplicit = isExplicit;
        }

        public bool IsExplicit { get; }

        public IReadOnlyCollection<InstalledPackage> Selected { get; }

        public IReadOnlyCollection<InstalledPackage> Unselected { get; }
    }

    public class PackagesSelectionChanged : PubSubEvent<PackagesSelectionChangedPayLoad>
    {
    }
}
