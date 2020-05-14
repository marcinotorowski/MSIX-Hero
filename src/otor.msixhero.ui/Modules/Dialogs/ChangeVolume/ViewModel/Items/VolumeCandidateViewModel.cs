using otor.msixhero.lib.Domain.Appx.Volume;
using otor.msixhero.ui.ViewModel;

namespace otor.msixhero.ui.Modules.Dialogs.ChangeVolume.ViewModel.Items
{
    public class VolumeCandidateViewModel : NotifyPropertyChanged
    {
        public VolumeCandidateViewModel(AppxVolume model)
        {
            this.Model = model;
        }

        public AppxVolume Model { get; }

        public string PackageStorePath => this.Model.PackageStorePath;

        public string Name => this.Model.Name;
        
        public long SizeTaken => this.Model.Capacity - this.Model.AvailableFreeSpace;

        public long TotalSize => this.Model.Capacity;

        public string DiskLabel => this.Model.DiskLabel;

        public bool IsOffline => this.Model.IsOffline;
    }
}
