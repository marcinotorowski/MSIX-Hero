// MSIX Hero
// Copyright (C) 2022 Marcin Otorowski
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// Full notice:
// https://github.com/marcinotorowski/msix-hero/blob/develop/LICENSE.md

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Hero.Commands;
using Otor.MsixHero.App.Hero.Commands.Volumes;
using Otor.MsixHero.App.Hero.Executor;
using Otor.MsixHero.App.Mvvm.Progress;
using Otor.MsixHero.Appx.Volumes;
using Otor.MsixHero.Appx.Volumes.Entities;
using Otor.MsixHero.Elevation;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Modularity;
using Prism.Services.Dialogs;

namespace Otor.MsixHero.App.Modules.VolumeManagement
{
    public class VolumeManagementHandler
    {
        private readonly IMsixHeroApplication _application;
        private readonly IInteractionService _interactionService;
        private readonly IDialogService _dialogService;
        private readonly IModuleManager _moduleManager;
        private readonly IConfigurationService _configurationService;
        private readonly IUacElevation _uacClient;
        private readonly IBusyManager _busyManager;

        public VolumeManagementHandler(
            UIElement parent, 
            IMsixHeroApplication application, 
            IInteractionService interactionService,
            IUacElevation uacClient,
            IBusyManager busyManager,
            IDialogService dialogService,
            IModuleManager moduleManager)
        {
            this._application = application;
            this._interactionService = interactionService;
            this._dialogService = dialogService;
            this._moduleManager = moduleManager;
            this._configurationService = application.ConfigurationService;
            this._uacClient = uacClient;
            this._busyManager = busyManager;
            
            parent.CommandBindings.Add(new CommandBinding(NavigationCommands.Refresh, this.OnRefresh));
            parent.CommandBindings.Add(new CommandBinding(ApplicationCommands.New, this.OnNew));
            parent.CommandBindings.Add(new CommandBinding(ApplicationCommands.Delete, this.OnDelete, this.CanDelete));
            parent.CommandBindings.Add(new CommandBinding(MsixHeroRoutedUICommands.SetVolumeAsDefault, this.OnSetVolumeAsDefault, this.CanSetVolumeAsDefault));
            parent.CommandBindings.Add(new CommandBinding(MsixHeroRoutedUICommands.MountVolume, this.OnMountVolume, this.CanMountVolume));
            parent.CommandBindings.Add(new CommandBinding(MsixHeroRoutedUICommands.DismountVolume, this.OnDismountVolume, this.CanDismountVolume));
        }


        private async void OnDelete(object sender, ExecutedRoutedEventArgs obj)
        {
            if (this._application.ApplicationState.Volumes.SelectedVolumes.Count != 1)
            {
                return;
            }

            var selected = this._application.ApplicationState.Volumes.SelectedVolumes.First();
            if (selected == null || selected.IsDefault)
            {
                return;
            }

            var config = await this._configurationService.GetCurrentConfigurationAsync().ConfigureAwait(false);
            if (config.UiConfiguration?.ConfirmDeletion == true)
            {
                var options = new List<string>
                {
                    Resources.Localization.Volumes_RemoveSelected,
                    Resources.Localization.Volumes_DoNotRemove
                };

                if (this._interactionService.ShowMessage(string.Format(Resources.Localization.Volumes_ConfirmRemoval, selected.PackageStorePath), options) != 0)
                {
                    return;
                }
            }

            try
            {
                var manager = this._uacClient.AsAdministrator<IAppxVolumeService>();

                var context = this._busyManager.Begin(OperationType.VolumeLoading);
                try
                {
                    foreach (var volume in this._application.ApplicationState.Volumes.SelectedVolumes.ToArray())
                    {
                        await manager.Delete(volume, CancellationToken.None, context).ConfigureAwait(false);
                    }
                }
                finally
                {
                    this._busyManager.End(context);
                }

                await this._application.CommandExecutor
                    .WithBusyManager(this._busyManager, OperationType.VolumeLoading)
                    .Invoke<GetVolumesCommand, IList<AppxVolume>>(this, new GetVolumesCommand()).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                this._interactionService.ShowError(Resources.Localization.Volumes_RemovalError, exception);
            }
        }

        private async void OnSetVolumeAsDefault(object sender, ExecutedRoutedEventArgs eventArgs)
        {
            if (this._application.ApplicationState.Volumes.SelectedVolumes.Count != 1)
            {
                return;
            }

            if (this._application.ApplicationState.Volumes.SelectedVolumes.First().IsDefault)
            {
                return;
            }

            var context = this._busyManager.Begin();
            try
            {
                context.Message = Resources.Localization.Volumes_SettingDefault;
                var manager = this._uacClient.AsAdministrator<IAppxVolumeService>();
                await manager.SetDefault(this._application.ApplicationState.Volumes.SelectedVolumes.First(), CancellationToken.None, context).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                this._interactionService.ShowError(Resources.Localization.Volumes_SettingDefaultError, exception);
            }
            finally
            {
                this._busyManager.End(context);
            }

            this.OnRefresh(sender, eventArgs);
        }

        private async void OnMountVolume(object sender, ExecutedRoutedEventArgs eventArgs)
        {
            if (this._application.ApplicationState.Volumes.SelectedVolumes.Count != 1)
            {
                return;
            }

            var selected = this._application.ApplicationState.Volumes.SelectedVolumes.First();
            if (selected == null || selected.IsDefault)
            {
                return;
            }

            var isOnline = !selected.IsOffline;
            if (isOnline)
            {
                return;
            }

            var context = this._busyManager.Begin();
            try
            {
                context.Message = Resources.Localization.Volumes_Mounting;
                var manager = this._uacClient.AsAdministrator<IAppxVolumeService>();
                await manager.Mount(this._application.ApplicationState.Volumes.SelectedVolumes.First(), CancellationToken.None, context).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                this._interactionService.ShowError(Resources.Localization.Volumes_MountingError, exception);
            }
            finally
            {
                this._busyManager.End(context);
            }

            this.OnRefresh(sender, eventArgs);
        }

        private async void OnDismountVolume(object sender, ExecutedRoutedEventArgs eventArgs)
        {
            if (this._application.ApplicationState.Volumes.SelectedVolumes.Count != 1)
            {
                return;
            }

            var selected = this._application.ApplicationState.Volumes.SelectedVolumes.First();
            if (selected == null || selected.IsDefault)
            {
                return;
            }

            var isOffline = selected.IsOffline;
            if (isOffline)
            {
                return;
            }

            var context = this._busyManager.Begin();
            try
            {
                context.Message = Resources.Localization.Volumes_Dismounting;
                var manager = this._uacClient.AsAdministrator<IAppxVolumeService>();
                await manager.Dismount(this._application.ApplicationState.Volumes.SelectedVolumes.First(), CancellationToken.None, context).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                this._interactionService.ShowError(Resources.Localization.Volumes_DismountingError, exception);
            }
            finally
            {
                this._busyManager.End(context);
            }

            this.OnRefresh(sender, eventArgs);
        }

        private void CanDelete(object sender, CanExecuteRoutedEventArgs eventArgs)
        {
            if (this._application.ApplicationState.Volumes.SelectedVolumes.Count != 1)
            {
                eventArgs.CanExecute = false;
            }
            else if (this._application.ApplicationState.Volumes.SelectedVolumes.First().IsDefault)
            {
                eventArgs.CanExecute = false;
            }
            else
            {
                eventArgs.CanExecute = this._application.ApplicationState.Volumes.AllVolumes.Count > 1;
            }
        }

        private void CanSetVolumeAsDefault(object sender, CanExecuteRoutedEventArgs eventArgs)
        {
            if (this._application.ApplicationState.Volumes.SelectedVolumes.Count != 1)
            {
                eventArgs.CanExecute = false;
            }
            else
            {
                var selected = this._application.ApplicationState.Volumes.SelectedVolumes.First();
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
            if (this._application.ApplicationState.Volumes.SelectedVolumes.Count != 1)
            {
                eventArgs.CanExecute = false;
            }
            else
            {
                var selected = this._application.ApplicationState.Volumes.SelectedVolumes.First();
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
            if (this._application.ApplicationState.Volumes.SelectedVolumes.Count != 1)
            {
                eventArgs.CanExecute = false;
            }
            else
            {

                var selected = this._application.ApplicationState.Volumes.SelectedVolumes.First();
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
            var executor = this._application.CommandExecutor
                .WithErrorHandling(this._interactionService, true)
                .WithBusyManager(this._busyManager, OperationType.VolumeLoading);

            await executor.Invoke<GetVolumesCommand, IList<AppxVolume>>(this, new GetVolumesCommand(), CancellationToken.None).ConfigureAwait(false);
        }

        private void OnNew(object sender, ExecutedRoutedEventArgs args)
        {
            this._moduleManager.LoadModule(ModuleNames.Dialogs.Volumes);
            this._dialogService.ShowDialog(NavigationPaths.DialogPaths.VolumesNewVolume, new DialogParameters(), this.OnDialogClosed);
        }

        private void OnDialogClosed(IDialogResult obj)
        {
        }
    }
}
