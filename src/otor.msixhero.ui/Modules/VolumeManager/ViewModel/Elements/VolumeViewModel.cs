using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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

        public bool IsDefault
        {
            get => this.Model.IsDefault;
        }

        public string Name
        {
            get => this.Model.Name;
        }

        public string PackageStorePath
        {
            get => this.Model.PackageStorePath;
        }

        protected override bool TrySelect()
        {
            if (this.StateManager.CurrentState.Volumes.SelectedItems.Contains(this.Model))
            {
                return false;
            }

            this.StateManager.CommandExecutor.ExecuteAsync(new SelectVolumes(this.Model, SelectionMode.AddToSelection));
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
