using System.Collections.Generic;
using Otor.MsixHero.Appx.Volumes.Entities;

namespace Otor.MsixHero.Lib.Domain.State
{
    public interface IVolumeListState
    {
        bool ShowSidebar { get; }

        string SearchKey { get; }

        IReadOnlyCollection<AppxVolume> HiddenItems { get; }

        IReadOnlyCollection<AppxVolume> VisibleItems { get; }

        IReadOnlyCollection<AppxVolume> SelectedItems { get; }
    }
}