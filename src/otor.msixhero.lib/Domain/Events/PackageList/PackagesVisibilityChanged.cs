using System.Collections.Generic;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;
using Prism.Events;

namespace Otor.MsixHero.Lib.Domain.Events.PackageList
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
