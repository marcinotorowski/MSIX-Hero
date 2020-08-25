using System.Collections.Generic;
using Otor.MsixHero.Appx.Volumes.Entities;

namespace Otor.MsixHero.Lib.Domain.Events.Volumes
{
    public class VolumesSelectionChangedPayLoad
    {
        public VolumesSelectionChangedPayLoad(IReadOnlyCollection<AppxVolume> selected, IReadOnlyCollection<AppxVolume> unselected, bool isExplicit)
        {
            Selected = selected;
            Unselected = unselected;
            IsExplicit = isExplicit;
        }

        public bool IsExplicit { get; }

        public IReadOnlyCollection<AppxVolume> Selected { get; }

        public IReadOnlyCollection<AppxVolume> Unselected { get; }
    }
}