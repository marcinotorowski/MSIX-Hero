using Otor.MsixHero.Appx.Volumes.Entities;

namespace Otor.MsixHero.Ui.Modules.VolumeManager.ViewModel.Elements
{
    public class VolumeViewModel : SelectableViewModel<AppxVolume>
    {
        public VolumeViewModel(AppxVolume model) : base(model)
        {
        }

        public bool IsDefault => this.Model.IsDefault;

        public string Name => this.Model.Name;

        public bool IsOffline => this.Model.IsOffline;

        public long SpaceTaken => this.Model.Capacity - this.Model.AvailableFreeSpace;

        public long Capacity => this.Model.Capacity;

        public string Label => this.Model.DiskLabel;
        
        public string PackageStorePath => this.Model.PackageStorePath;
    }
}
