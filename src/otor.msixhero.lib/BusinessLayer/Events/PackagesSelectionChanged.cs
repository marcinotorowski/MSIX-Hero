using System.Collections.Generic;
using Prism.Events;

namespace otor.msixhero.lib.BusinessLayer.Events
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
