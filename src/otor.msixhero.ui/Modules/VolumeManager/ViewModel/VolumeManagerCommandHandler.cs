using System.Linq;
using System.Windows.Input;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Commands.Volumes;
using otor.msixhero.ui.Commands.RoutedCommand;

namespace otor.msixhero.ui.Modules.VolumeManager.ViewModel
{
    public class VolumeManagerCommandHandler
    {
        private readonly IApplicationStateManager stateManager;

        public VolumeManagerCommandHandler(IApplicationStateManager stateManager)
        {
            this.stateManager = stateManager;
            this.Refresh = new DelegateCommand(this.RefreshExecute);
            this.New = new DelegateCommand(this.NewExecute);
            this.Delete = new DelegateCommand(this.DeleteExecute, this.CanDeleteExecute);
            this.SetVolumeAsDefault = new DelegateCommand(this.SetVolumeAsDefaultExecute, this.CanSetVolumeAsDefaultExecute);
        }

        private void NewExecute(object obj)
        {
        }

        private void DeleteExecute(object obj)
        {
            if (this.stateManager.CurrentState.Volumes.SelectedItems.Count != 1)
            {
                return;
            }

            if (this.stateManager.CurrentState.Volumes.SelectedItems.First().IsDefault)
            {
                return;
            }

            this.stateManager.CommandExecutor.ExecuteAsync(new RemoveVolume(this.stateManager.CurrentState.Volumes.SelectedItems.First()));
        }

        private void SetVolumeAsDefaultExecute(object obj)
        {
        }

        private bool CanDeleteExecute(object obj)
        {
            if (this.stateManager.CurrentState.Volumes.SelectedItems.Count != 1)
            {
                return false;
            }

            if (this.stateManager.CurrentState.Volumes.SelectedItems.First().IsDefault)
            {
                return false;
            }

            return this.stateManager.CurrentState.Volumes.VisibleItems.Count + this.stateManager.CurrentState.Volumes.HiddenItems.Count > 1;
        }

        private bool CanSetVolumeAsDefaultExecute(object obj)
        {
            if (this.stateManager.CurrentState.Volumes.SelectedItems.Count != 1)
            {
                return false;
            }

            return !this.stateManager.CurrentState.Volumes.SelectedItems.First().IsDefault;
        }

        public ICommand Refresh { get; }

        public ICommand SetVolumeAsDefault { get; }
        
        public ICommand Delete { get; }

        public ICommand New { get; }

        private void RefreshExecute(object obj)
        {
            this.stateManager.CommandExecutor.ExecuteAsync(new GetVolumes());
        }
    }
}
