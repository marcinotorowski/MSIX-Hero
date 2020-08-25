using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using Otor.MsixHero.Appx.Volumes;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation.Enums;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Lib.Infrastructure;
using Otor.MsixHero.Lib.Infrastructure.Progress;
using Otor.MsixHero.Ui.Commands.RoutedCommand;
using Otor.MsixHero.Ui.Hero;
using Otor.MsixHero.Ui.Hero.Commands.Volumes;
using Otor.MsixHero.Ui.Hero.Executor;
using Prism.Services.Dialogs;

namespace Otor.MsixHero.Ui.Modules.VolumeManager.ViewModel
{
    public class VolumeManagerCommandHandler
    {
        private readonly IMsixHeroApplication application;
        private readonly ISelfElevationProxyProvider<IAppxVolumeManager> volumeManagerFactory;
        private readonly IConfigurationService configurationService;
        private readonly IInteractionService interactionService;
        private readonly IDialogService dialogService;
        private readonly IBusyManager busyManager;

        public VolumeManagerCommandHandler(
            IMsixHeroApplication application,
            ISelfElevationProxyProvider<IAppxVolumeManager> volumeManagerFactory,
            IConfigurationService configurationService,
            IInteractionService interactionService,
            IDialogService dialogService, 
            IBusyManager busyManager)
        {
            this.application = application;
            this.volumeManagerFactory = volumeManagerFactory;
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
            if (this.application.ApplicationState.Volumes.SelectedVolumes.Count != 1)
            {
                return;
            }
            
            var selected = this.application.ApplicationState.Volumes.SelectedVolumes.First();
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
            
            try
            {
                var manager = await this.volumeManagerFactory.GetProxyFor(SelfElevationLevel.AsAdministrator, CancellationToken.None).ConfigureAwait(false);

                var context = this.busyManager.Begin(OperationType.VolumeLoading);
                try
                {
                    foreach (var volume in this.application.ApplicationState.Volumes.SelectedVolumes.ToArray())
                    {
                        await manager.Delete(volume, CancellationToken.None, context).ConfigureAwait(false);
                    }
                }
                finally
                {
                    this.busyManager.End(context);
                }

                await this.application.CommandExecutor
                    .WithBusyManager(this.busyManager, OperationType.VolumeLoading)
                    .Invoke(this, new GetVolumesCommand()).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                this.interactionService.ShowError("Could not delete the volume.", exception);
            }
        }

        private async void SetVolumeAsDefaultExecute(object obj)
        {
            if (this.application.ApplicationState.Volumes.SelectedVolumes.Count != 1)
            {
                return;
            }
            
            if (this.application.ApplicationState.Volumes.SelectedVolumes.First().IsDefault)
            {
                return;
            }
            
            var context = this.busyManager.Begin();
            try
            {
                context.Message = "Setting the default volume...";
                var manager = await this.volumeManagerFactory.GetProxyFor(SelfElevationLevel.AsAdministrator, CancellationToken.None).ConfigureAwait(false);
                await manager.SetDefault(this.application.ApplicationState.Volumes.SelectedVolumes.First(), CancellationToken.None, context).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                this.interactionService.ShowError("Could not set the volume as the default volume.", exception);
            }
            finally
            {
                this.busyManager.End(context);
            }
            
            this.RefreshExecute(obj);
        }

        private async void MountVolumeExecute(object obj)
        {
            if (this.application.ApplicationState.Volumes.SelectedVolumes.Count != 1)
            {
                return;
            }
            
            var selected = this.application.ApplicationState.Volumes.SelectedVolumes.First();
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
                var manager = await this.volumeManagerFactory.GetProxyFor(SelfElevationLevel.AsAdministrator, CancellationToken.None).ConfigureAwait(false);
                await manager.Mount(this.application.ApplicationState.Volumes.SelectedVolumes.First(), CancellationToken.None, context).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                this.interactionService.ShowError("Could not mount the volume.", exception);
            }
            finally
            {
                this.busyManager.End(context);
            }
            
            this.RefreshExecute(obj);
        }

        private async void DismountVolumeExecute(object obj)
        {
            if (this.application.ApplicationState.Volumes.SelectedVolumes.Count != 1)
            {
                return;
            }
            
            var selected = this.application.ApplicationState.Volumes.SelectedVolumes.First();
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
                var manager = await this.volumeManagerFactory.GetProxyFor(SelfElevationLevel.AsAdministrator, CancellationToken.None).ConfigureAwait(false);
                await manager.Dismount(this.application.ApplicationState.Volumes.SelectedVolumes.First(), CancellationToken.None, context).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                this.interactionService.ShowError("Could not dismount the volume.", exception);
            }
            finally
            {
                this.busyManager.End(context);
            }
            
            this.RefreshExecute(obj);
        }

        private bool CanDeleteExecute(object obj)
        {
            if (this.application.ApplicationState.Volumes.SelectedVolumes.Count != 1)
            {
                return false;
            }
            
            if (this.application.ApplicationState.Volumes.SelectedVolumes.First().IsDefault)
            {
                return false;
            }
            
            return this.application.ApplicationState.Volumes.AllVolumes.Count > 1;
        }

        private bool CanSetVolumeAsDefaultExecute(object obj)
        {
            if (this.application.ApplicationState.Volumes.SelectedVolumes.Count != 1)
            {
                return false;
            }
            
            var selected = this.application.ApplicationState.Volumes.SelectedVolumes.First();
            if (selected == null)
            {
                return false;
            }
            
            return !selected.IsDefault && !selected.IsOffline;
        }

        private bool CanMountVolumeExecute(object obj)
        {
            if (this.application.ApplicationState.Volumes.SelectedVolumes.Count != 1)
            {
                return false;
            }
            
            var selected = this.application.ApplicationState.Volumes.SelectedVolumes.First();
            if (selected == null || selected.IsDefault)
            {
                return false;
            }
            
            return selected.IsOffline;
        }

        private bool CanDismountVolumeExecute(object obj)
        {
            if (this.application.ApplicationState.Volumes.SelectedVolumes.Count != 1)
            {
                return false;
            }
            
            var selected = this.application.ApplicationState.Volumes.SelectedVolumes.First();
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
            var executor = this.application.CommandExecutor
                .WithErrorHandling(this.interactionService, true)
                .WithBusyManager(this.busyManager, OperationType.VolumeLoading);

            await executor.Invoke(this, new GetVolumesCommand(), CancellationToken.None).ConfigureAwait(false);
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
