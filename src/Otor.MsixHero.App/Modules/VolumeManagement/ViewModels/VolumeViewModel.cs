using Otor.MsixHero.Appx.Volumes.Entities;

namespace Otor.MsixHero.App.Modules.VolumeManagement.ViewModels
{
    public class VolumeViewModel
    {
        public VolumeViewModel(AppxVolume model)
        {
            this.Model = model;
        }

        public AppxVolume Model { get; }

        public bool IsDefault => this.Model.IsDefault;

        public string Name => this.Model.Name;

        public bool IsOffline => this.Model.IsOffline;

        public long SpaceTaken => this.Model.Capacity - this.Model.AvailableFreeSpace;

        public long Capacity => this.Model.Capacity;

        public string Label => this.Model.DiskLabel;
        
        public string PackageStorePath => this.Model.PackageStorePath;
    }
}
