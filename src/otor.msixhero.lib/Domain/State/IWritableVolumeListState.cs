using System.Collections.Generic;
using Otor.MsixHero.Appx.Volumes.Entities;

namespace Otor.MsixHero.Lib.Domain.State
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