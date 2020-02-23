using System.Collections.Generic;
using otor.msixhero.lib.Domain.Appx.Volume;
using Prism.Events;

namespace otor.msixhero.lib.Domain.Events.Volumes
{
    public class VolumesSelectionChangedPayLoad
    {
        public VolumesSelectionChangedPayLoad(IReadOnlyCollection<AppxVolume> selected, IReadOnlyCollection<AppxVolume> unselected)
        {
            Selected = selected;
            Unselected = unselected;
        }

        public IReadOnlyCollection<AppxVolume> Selected { get; }

        public IReadOnlyCollection<AppxVolume> Unselected { get; }
    }

    public class VolumesSelectionChanged : PubSubEvent<VolumesSelectionChangedPayLoad>
    {
    }
}
