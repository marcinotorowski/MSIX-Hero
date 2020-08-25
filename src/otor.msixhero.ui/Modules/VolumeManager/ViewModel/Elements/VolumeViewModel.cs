using System.Linq;
using Otor.MsixHero.Appx.Volumes.Entities;
using Otor.MsixHero.Ui.Hero;
using Otor.MsixHero.Ui.Hero.Commands.Volumes;

namespace Otor.MsixHero.Ui.Modules.VolumeManager.ViewModel.Elements
{
    public class VolumeViewModel : SelectableViewModel<AppxVolume>
    {
        private readonly IMsixHeroApplication app;

        public VolumeViewModel(IMsixHeroApplication app, AppxVolume model, bool isSelected = false) : base(model, isSelected)
        {
            this.app = app;
        }

        public bool IsDefault => this.Model.IsDefault;

        public string Name => this.Model.Name;

        public bool IsOffline => this.Model.IsOffline;

        public long SpaceTaken => this.Model.Capacity - this.Model.AvailableFreeSpace;

        public long Capacity => this.Model.Capacity;

        public string Label => this.Model.DiskLabel;
        
        public string PackageStorePath => this.Model.PackageStorePath;

        protected override bool TrySelect()
        {
            var selected = this.app.ApplicationState.Volumes.SelectedVolumes.Select(p => p.PackageStorePath).Union(new[] { this.PackageStorePath });
            this.app.CommandExecutor.Invoke(this, new SelectVolumesCommand(selected));
            return true;
        }

        protected override bool TryUnselect()
        {
            var selected = this.app.ApplicationState.Volumes.SelectedVolumes.Select(p => p.PackageStorePath).Except(new[] { this.PackageStorePath });
            this.app.CommandExecutor.Invoke(this, new SelectVolumesCommand(selected));
            return true;
        }
    }
}
