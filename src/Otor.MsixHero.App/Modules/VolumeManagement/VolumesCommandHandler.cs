using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Hero.Commands.Volumes;
using Otor.MsixHero.App.Hero.Executor;
using Otor.MsixHero.Appx.Volumes;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation.Enums;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Lib.Infrastructure.Progress;
using Prism.Modularity;
using Prism.Services.Dialogs;

namespace Otor.MsixHero.App.Modules.VolumeManagement
{
    public class VolumeManagementHandler
    {
        private readonly IMsixHeroApplication application;
        private readonly IInteractionService interactionService;
        private readonly IDialogService dialogService;
        private readonly IModuleManager moduleManager;
        private readonly IConfigurationService configurationService;
        private readonly ISelfElevationProxyProvider<IAppxVolumeManager> volumeManagerProvider;
        private readonly IBusyManager busyManager;

        public VolumeManagementHandler(
            UIElement parent, 
            IMsixHeroApplication application, 
            IInteractionService interactionService,
            IConfigurationService configurationService,
            ISelfElevationProxyProvider<IAppxVolumeManager> volumeManagerProvider,
            IBusyManager busyManager,
            IDialogService dialogService,
            IModuleManager moduleManager)
        {
            this.application = application;
            this.interactionService = interactionService;
            this.dialogService = dialogService;
            this.moduleManager = moduleManager;
            this.configurationService = configurationService;
            this.volumeManagerProvider = volumeManagerProvider;
            this.busyManager = busyManager;
            
            parent.CommandBindings.Add(new CommandBinding(NavigationCommands.Refresh, this.OnRefresh));
            parent.CommandBindings.Add(new CommandBinding(ApplicationCommands.New, this.OnNew));
            parent.CommandBindings.Add(new CommandBinding(ApplicationCommands.Delete, this.OnDelete, this.CanDelete));
            parent.CommandBindings.Add(new CommandBinding(MsixHeroCommands.SetVolumeAsDefault, this.OnSetVolumeAsDefault, this.CanSetVolumeAsDefault));
            parent.CommandBindings.Add(new CommandBinding(MsixHeroCommands.MountVolume, this.OnMountVolume, this.CanMountVolume));
            parent.CommandBindings.Add(new CommandBinding(MsixHeroCommands.DismountVolume, this.OnDismountVolume, this.CanDismountVolume));
        }


        private async void OnDelete(object sender, ExecutedRoutedEventArgs obj)
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
                var manager = await this.volumeManagerProvider.GetProxyFor(SelfElevationLevel.AsAdministrator, CancellationToken.None).ConfigureAwait(false);

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

        private async void OnSetVolumeAsDefault(object sender, ExecutedRoutedEventArgs eventArgs)
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
                var manager = await this.volumeManagerProvider.GetProxyFor(SelfElevationLevel.AsAdministrator, CancellationToken.None).ConfigureAwait(false);
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

            this.OnRefresh(sender, eventArgs);
        }

        private async void OnMountVolume(object sender, ExecutedRoutedEventArgs eventArgs)
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
                var manager = await this.volumeManagerProvider.GetProxyFor(SelfElevationLevel.AsAdministrator, CancellationToken.None).ConfigureAwait(false);
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

            this.OnRefresh(sender, eventArgs);
        }

        private async void OnDismountVolume(object sender, ExecutedRoutedEventArgs eventArgs)
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
                var manager = await this.volumeManagerProvider.GetProxyFor(SelfElevationLevel.AsAdministrator, CancellationToken.None).ConfigureAwait(false);
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

            this.OnRefresh(sender, eventArgs);
        }

        private void CanDelete(object sender, CanExecuteRoutedEventArgs eventArgs)
        {
            if (this.application.ApplicationState.Volumes.SelectedVolumes.Count != 1)
            {
                eventArgs.CanExecute = false;
            }
            else if (this.application.ApplicationState.Volumes.SelectedVolumes.First().IsDefault)
            {
                eventArgs.CanExecute = false;
            }
            else
            {
                eventArgs.CanExecute = this.application.ApplicationState.Volumes.AllVolumes.Count > 1;
            }
        }

        private void CanSetVolumeAsDefault(object sender, CanExecuteRoutedEventArgs eventArgs)
        {
            if (this.application.ApplicationState.Volumes.SelectedVolumes.Count != 1)
            {
                eventArgs.CanExecute = false;
            }
            else
            {
                var selected = this.application.ApplicationState.Volumes.SelectedVolumes.First();
                if (selected == null)
                {
                    eventArgs.CanExecute = false;
                }
                else
                {
                    eventArgs.CanExecute = !selected.IsDefault && !selected.IsOffline;
                }
            }
        }

        private void CanMountVolume(object sender, CanExecuteRoutedEventArgs eventArgs)
        {
            if (this.application.ApplicationState.Volumes.SelectedVolumes.Count != 1)
            {
                eventArgs.CanExecute = false;
            }
            else
            {
                var selected = this.application.ApplicationState.Volumes.SelectedVolumes.First();
                if (selected == null || selected.IsDefault)
                {
                    eventArgs.CanExecute = false;
                }
                else
                {
                    eventArgs.CanExecute = selected.IsOffline;
                }
            }
        }

        private void CanDismountVolume(object sender, CanExecuteRoutedEventArgs eventArgs)
        {
            if (this.application.ApplicationState.Volumes.SelectedVolumes.Count != 1)
            {
                eventArgs.CanExecute = false;
            }
            else
            {

                var selected = this.application.ApplicationState.Volumes.SelectedVolumes.First();
                if (selected == null || selected.IsDefault)
                {
                    eventArgs.CanExecute = false;
                }
                else
                {
                    eventArgs.CanExecute = !selected.IsOffline;
                }
            }
        }
        
        private async void OnRefresh(object sender, ExecutedRoutedEventArgs args)
        {
            var executor = this.application.CommandExecutor
                .WithErrorHandling(this.interactionService, true)
                .WithBusyManager(this.busyManager, OperationType.VolumeLoading);

            await executor.Invoke(this, new GetVolumesCommand(), CancellationToken.None).ConfigureAwait(false);
        }

        private void OnNew(object sender, ExecutedRoutedEventArgs args)
        {
            this.moduleManager.LoadModule(ModuleNames.Dialogs.Volumes);
            this.dialogService.ShowDialog(NavigationPaths.DialogPaths.VolumesNewVolume, new DialogParameters(), this.OnDialogClosed);
        }

        private void OnDialogClosed(IDialogResult obj)
        {
        }
    }
}
