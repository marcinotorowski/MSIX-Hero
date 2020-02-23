using System.Collections.Generic;
using otor.msixhero.lib.Domain.Appx.Volume;
using Prism.Events;

namespace otor.msixhero.lib.Domain.Events.Volumes
{
    public class VolumesVisibilityChangedPayLoad
    {
        public VolumesVisibilityChangedPayLoad(IReadOnlyCollection<AppxVolume> newVisible, IReadOnlyCollection<AppxVolume> newHidden)
        {
            this.NewVisible = newVisible;
            this.NewHidden = newHidden;
        }

        public IReadOnlyCollection<AppxVolume> NewVisible { get; }

        public IReadOnlyCollection<AppxVolume> NewHidden { get; }
    }

    public class VolumesVisibilityChanged : PubSubEvent<VolumesVisibilityChangedPayLoad>
    {
    }
}
