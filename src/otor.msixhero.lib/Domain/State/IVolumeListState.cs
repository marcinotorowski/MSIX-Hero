using System.Collections.Generic;
using otor.msixhero.lib.Domain.Appx.Volume;

namespace otor.msixhero.lib.Domain.State
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