using System.Collections.Generic;
using otor.msixhero.lib.Domain.Appx.Volume;

namespace otor.msixhero.lib.Domain.State
{
    public interface IWritableVolumeListState
    {
        bool ShowSidebar { get; set; }

        string SearchKey { get; set; }

        List<AppxVolume> HiddenItems { get; }

        List<AppxVolume> VisibleItems { get; }

        List<AppxVolume> SelectedItems { get; }
    }
}