using System;
using System.Collections.Generic;
using System.Text;
using otor.msihero.lib;
using Prism.Events;

namespace MSI_Hero.Domain.Events
{
    public class PackagesSelectionChangedPayLoad
    {
        public PackagesSelectionChangedPayLoad(IReadOnlyCollection<Package> selected, IReadOnlyCollection<Package> unselected)
        {
            Selected = selected;
            Unselected = unselected;
        }

        public IReadOnlyCollection<Package> Selected { get; }

        public IReadOnlyCollection<Package> Unselected { get; }
    }

    public class PackagesSelectionChanged : PubSubEvent<PackagesSelectionChangedPayLoad>
    {
    }
}
