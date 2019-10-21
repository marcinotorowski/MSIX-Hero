using System;
using System.Collections.Generic;
using otor.msihero.lib;
using Prism.Events;

namespace MSI_Hero.Domain.Events
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
