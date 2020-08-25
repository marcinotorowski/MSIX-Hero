using System.Collections.Generic;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;
using Prism.Events;

namespace Otor.MsixHero.Lib.Domain.Events.PackageList
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
