using System.Collections.Generic;
using Otor.MsixHero.Appx.Volumes.Entities;
using Prism.Events;

namespace Otor.MsixHero.Lib.Domain.Events.Volumes
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
