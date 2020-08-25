using Otor.MsixHero.Appx.Volumes.Entities;
using Otor.MsixHero.Ui.ViewModel;

namespace Otor.MsixHero.Ui.Modules.Dialogs.NewVolume.ViewModel.Items
{
    public class VolumeCandidateViewModel : NotifyPropertyChanged
    {
        public VolumeCandidateViewModel(AppxVolume model)
        {
            this.Model = model;
        }

        public AppxVolume Model { get; }

        public string PackageStorePath => this.Model.PackageStorePath;
        
        public long SizeTaken => this.Model.Capacity - this.Model.AvailableFreeSpace;

        public long TotalSize => this.Model.Capacity;

        public string DiskLabel => this.Model.DiskLabel;

        public bool IsOffline => this.Model.IsOffline;
    }
}
