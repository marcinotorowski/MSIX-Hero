using System.Linq;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Appx.Volume;
using otor.msixhero.lib.Domain.Commands.Packages.Grid;
using otor.msixhero.lib.Domain.Commands.Volumes;

namespace otor.msixhero.ui.Modules.VolumeManager.ViewModel.Elements
{
    public class VolumeViewModel : SelectableViewModel<AppxVolume>
    {
        public VolumeViewModel(AppxVolume model, IApplicationStateManager stateManager, bool isSelected = false) : base(model, stateManager, isSelected)
        {
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
            if (this.StateManager.CurrentState.Volumes.SelectedItems.Contains(this.Model))
            {
                return false;
            }

            this.StateManager.CommandExecutor.ExecuteAsync(new SelectVolumes(this.Model, SelectionMode.AddToSelection) { IsExplicit = true });
            return true;
        }

        protected override bool TryUnselect()
        {
            if (!this.StateManager.CurrentState.Volumes.SelectedItems.Contains(this.Model))
            {
                return false;
            }

            this.StateManager.CommandExecutor.ExecuteAsync(new SelectVolumes(this.Model, SelectionMode.RemoveFromSelection));
            return true;
        }
    }
}
