using System.Collections.Generic;
using otor.msixhero.lib.Domain.Appx.Volume;

namespace otor.msixhero.lib.Domain.State
{
    public class VolumeListState : IVolumeListState, IWritableVolumeListState
    {
        public VolumeListState()
        {
            this.ShowSidebar = true;

            this.VisibleItems = new List<AppxVolume>();
            this.HiddenItems = new List<AppxVolume>();
            this.SelectedItems = new List<AppxVolume>();
        }
        
        public bool ShowSidebar { get; set; }

        public string SearchKey { get; set; }

        public List<AppxVolume> VisibleItems { get; }

        public List<AppxVolume> HiddenItems { get; }

        public List<AppxVolume> SelectedItems { get; }

        IReadOnlyCollection<AppxVolume> IVolumeListState.HiddenItems => this.HiddenItems;

        IReadOnlyCollection<AppxVolume> IVolumeListState.VisibleItems => this.VisibleItems;

        IReadOnlyCollection<AppxVolume> IVolumeListState.SelectedItems => this.SelectedItems;
    }
}