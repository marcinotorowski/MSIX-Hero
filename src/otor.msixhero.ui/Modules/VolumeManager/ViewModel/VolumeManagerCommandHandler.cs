using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Commands.Volumes;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Commanding;
using otor.msixhero.lib.Infrastructure.Configuration;
using otor.msixhero.lib.Infrastructure.Progress;
using otor.msixhero.ui.Commands.RoutedCommand;
using Prism.Services.Dialogs;

namespace otor.msixhero.ui.Modules.VolumeManager.ViewModel
{
    public class VolumeManagerCommandHandler
    {
        private readonly IApplicationStateManager stateManager;
        private readonly IConfigurationService configurationService;
        private readonly IInteractionService interactionService;
        private readonly IDialogService dialogService;
        private readonly IBusyManager busyManager;

        public VolumeManagerCommandHandler(
            IApplicationStateManager stateManager, 
            IConfigurationService configurationService,
            IInteractionService interactionService,
            IDialogService dialogService, 
            IBusyManager busyManager)
        {
            this.stateManager = stateManager;
            this.configurationService = configurationService;
            this.interactionService = interactionService;
            this.dialogService = dialogService;
            this.busyManager = busyManager;
            this.Refresh = new DelegateCommand(this.RefreshExecute);
            this.New = new DelegateCommand(this.NewExecute);
            this.Delete = new DelegateCommand(this.DeleteExecute, this.CanDeleteExecute);
            this.MountVolume = new DelegateCommand(this.MountVolumeExecute, this.CanMountVolumeExecute);
            this.DismountVolume = new DelegateCommand(this.DismountVolumeExecute, this.CanDismountVolumeExecute);
            this.SetVolumeAsDefault = new DelegateCommand(this.SetVolumeAsDefaultExecute, this.CanSetVolumeAsDefaultExecute);
        }

        private async void DeleteExecute(object obj)
        {
            if (this.stateManager.CurrentState.Volumes.SelectedItems.Count != 1)
            {
                return;
            }

            var selected = this.stateManager.CurrentState.Volumes.SelectedItems.First();
            if (selected == null || selected.IsDefault)
            {
                return;
            }

            var config = await this.configurationService.GetCurrentConfigurationAsync().ConfigureAwait(false);
            if (config.UiConfiguration?.ConfirmDeletion == true)
            {
                var options = new List<string>
                {
                    "Remove selected volume",
                    "Do not remove"
                };

                if (this.interactionService.ShowMessage($"Are you sure you want to remove volume '{selected.PackageStorePath}'? Removing of volumes affects only registration and does not delete any physical files.", options) != 0)
                {
                    return;
                }
            }

            var context = this.busyManager.Begin(OperationType.VolumeLoading);
            try
            {
                await this.stateManager.CommandExecutor.ExecuteAsync(new RemoveVolume(this.stateManager.CurrentState.Volumes.SelectedItems.First()), CancellationToken.None, context).ConfigureAwait(false);
                await this.stateManager.CommandExecutor.ExecuteAsync(new GetVolumes(), CancellationToken.None, context).ConfigureAwait(false);
                
                if (this.stateManager.CurrentState.Volumes.SelectedItems.Any())
                {
                    var selection = SelectVolumes.CreateSingle(this.stateManager.CurrentState.Volumes.SelectedItems.First());
                    selection.IsExplicit = true;
                    await this.stateManager.CommandExecutor.ExecuteAsync(selection, CancellationToken.None, context).ConfigureAwait(false);
                }
            }
            catch (UserHandledException)
            {
            }
            finally
            {
                this.busyManager.End(context);
            }
        }

        private async void SetVolumeAsDefaultExecute(object obj)
        {
            if (this.stateManager.CurrentState.Volumes.SelectedItems.Count != 1)
            {
                return;
            }

            if (this.stateManager.CurrentState.Volumes.SelectedItems.First().IsDefault)
            {
                return;
            }

            var context = this.busyManager.Begin();
            try
            {
                context.Message = "Setting the default volume...";
                await this.stateManager.CommandExecutor.ExecuteAsync(new SetDefaultVolume(this.stateManager.CurrentState.Volumes.SelectedItems.First().PackageStorePath)).ConfigureAwait(false);
            }
            catch (UserHandledException)
            {
                return;
            }
            finally
            {
                this.busyManager.End(context);
            }

            this.RefreshExecute(obj);
        }

        private async void MountVolumeExecute(object obj)
        {
            if (this.stateManager.CurrentState.Volumes.SelectedItems.Count != 1)
            {
                return;
            }

            var selected = this.stateManager.CurrentState.Volumes.SelectedItems.First();
            if (selected == null || selected.IsDefault)
            {
                return;
            }

            var isOnline = !selected.IsOffline;
            if (isOnline)
            {
                return;
            }

            var context = this.busyManager.Begin();
            try
            {
                context.Message = "Mounting volume...";
                await this.stateManager.CommandExecutor.ExecuteAsync(new MountVolume(this.stateManager.CurrentState.Volumes.SelectedItems.First())).ConfigureAwait(false);
            }
            catch (UserHandledException)
            {
                return;
            }
            finally
            {
                this.busyManager.End(context);
            }

            this.RefreshExecute(obj);
        }

        private async void DismountVolumeExecute(object obj)
        {
            if (this.stateManager.CurrentState.Volumes.SelectedItems.Count != 1)
            {
                return;
            }

            var selected = this.stateManager.CurrentState.Volumes.SelectedItems.First();
            if (selected == null || selected.IsDefault)
            {
                return;
            }

            var isOffline = selected.IsOffline;
            if (isOffline)
            {
                return;
            }

            var context = this.busyManager.Begin();
            try
            {
                context.Message = "Dismounting volume...";
                await this.stateManager.CommandExecutor.ExecuteAsync(new DismountVolume(this.stateManager.CurrentState.Volumes.SelectedItems.First())).ConfigureAwait(false);
            }
            catch (UserHandledException)
            {
                return;
            }
            finally
            {
                this.busyManager.End(context);
            }

            this.RefreshExecute(obj);
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

            var selected = this.stateManager.CurrentState.Volumes.SelectedItems.First();
            if (selected == null)
            {
                return false;
            }

            return !selected.IsDefault && !selected.IsOffline;
        }

        private bool CanMountVolumeExecute(object obj)
        {
            if (this.stateManager.CurrentState.Volumes.SelectedItems.Count != 1)
            {
                return false;
            }

            var selected = this.stateManager.CurrentState.Volumes.SelectedItems.First();
            if (selected == null || selected.IsDefault)
            {
                return false;
            }

            return selected.IsOffline;
        }

        private bool CanDismountVolumeExecute(object obj)
        {
            if (this.stateManager.CurrentState.Volumes.SelectedItems.Count != 1)
            {
                return false;
            }

            var selected = this.stateManager.CurrentState.Volumes.SelectedItems.First();
            if (selected == null || selected.IsDefault)
            {
                return false;
            }

            return !selected.IsOffline;
        }

        public ICommand Refresh { get; }

        public ICommand SetVolumeAsDefault { get; }

        public ICommand MountVolume { get; }

        public ICommand DismountVolume { get; }
        
        public ICommand Delete { get; }

        public ICommand New { get; }

        private async void RefreshExecute(object obj)
        {
            var context = this.busyManager.Begin(OperationType.VolumeLoading);
            try
            {
                await this.stateManager.CommandExecutor.ExecuteAsync(new GetVolumes(), CancellationToken.None, context).ConfigureAwait(false);
            }
            catch (UserHandledException)
            {
            }
            finally
            {
                this.busyManager.End(context);
            }
        }

        private void NewExecute(object obj)
        {
            this.dialogService.ShowDialog(Constants.PathNewVolume, new DialogParameters(), this.OnDialogClosed);
        }

        private void OnDialogClosed(IDialogResult obj)
        {
        }
    }
}
