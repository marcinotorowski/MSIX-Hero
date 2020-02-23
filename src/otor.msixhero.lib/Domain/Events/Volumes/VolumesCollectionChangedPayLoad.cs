using System.Collections.Generic;
using otor.msixhero.lib.Domain.Appx.Volume;

namespace otor.msixhero.lib.Domain.Events.Volumes
{
    public class VolumesCollectionChangedPayLoad
    {
        public VolumesCollectionChangedPayLoad(CollectionChangeType type)
        {
            this.Type = type;

            this.NewVolumes = new List<AppxVolume>();
            this.OldVolumes = new List<AppxVolume>();
        }
        
        public CollectionChangeType Type { get; private set; }

        public IList<AppxVolume> NewVolumes { get; private set; }

        public IList<AppxVolume> OldVolumes { get; private set; }
    }
}