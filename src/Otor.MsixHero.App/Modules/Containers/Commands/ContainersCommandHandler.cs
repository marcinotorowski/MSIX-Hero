// MSIX Hero
// Copyright (C) 2024 Marcin Otorowski
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
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Hero.Commands.Containers;
using Otor.MsixHero.App.Hero.Executor;
using Otor.MsixHero.App.Modules.Dialogs.Packaging.SharedPackageContainer.Navigation;
using Otor.MsixHero.App.Mvvm.Progress;
using Otor.MsixHero.Appx.Packaging.SharedPackageContainer;
using Otor.MsixHero.Appx.Packaging.SharedPackageContainer.Entities;
using Otor.MsixHero.Elevation;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Commands;

namespace Otor.MsixHero.App.Modules.Containers.Commands
{
    public class ContainersCommandHandler
    {
        private readonly IUacElevation _uac;
        private readonly IMsixHeroApplication _application;
        private readonly IBusyManager _busyManager;
        private readonly PrismServices _prismServices;
        private readonly IInteractionService _interactionService;

        public ContainersCommandHandler(
            IUacElevation uac,
            IMsixHeroApplication application,
            IInteractionService interactionService,
            IBusyManager busyManager,
            PrismServices prismServices)
        {
            this._uac = uac;
            this._application = application;
            this._interactionService = interactionService;
            this._busyManager = busyManager;
            this._prismServices = prismServices;

            this.Refresh = new DelegateCommand(this.OnRefresh, this.CanRefresh);
            this.Add = new DelegateCommand(this.OnAdd, this.CanAdd);
            this.Copy = new DelegateCommand(this.OnCopy, this.CanCopy);
            this.Edit = new DelegateCommand<object>(this.OnEdit, this.CanEdit);
            this.Delete = new DelegateCommand(this.OnDelete, this.CanDelete);
            this.Reset = new DelegateCommand(this.OnReset, this.CanReset);
        }

        public ICommand Refresh { get; }
        
        public ICommand Copy { get; }

        public ICommand Add { get; }

        public ICommand Delete { get; }

        public ICommand Edit { get; }

        public ICommand Reset { get; }

        private async void OnRefresh()
        {
            var executor = this._application.CommandExecutor
                .WithErrorHandling(this._interactionService, true)
                .WithBusyManager(this._busyManager, OperationType.ContainerLoading);

            await executor.Invoke<GetSharedPackageContainersCommand, IList<SharedPackageContainer>>(this, new GetSharedPackageContainersCommand(), CancellationToken.None).ConfigureAwait(false);
        }

        private void OnAdd()
        {
            var dialogRequest = new NavigationRequest
            {
                Type = NavigationRequest.EditingType.New
            };

            this._prismServices.ModuleManager.LoadModule(ModuleNames.Dialogs.Packaging);
            this._prismServices.DialogService.ShowDialog(NavigationPaths.DialogPaths.PackagingSharedPackageContainer, dialogRequest.ToDialogParameters(), _ => { });
        }

        private async void OnDelete()
        {
            var context = this._busyManager.Begin(OperationType.ContainerLoading);
            context.Message = "Deleting container...";

            try
            {
                await this._uac.AsAdministrator<IAppxSharedPackageContainerService>().Reset(this._application.ApplicationState.Containers.SelectedContainer?.Name).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                this._interactionService.ShowError("The container could not be reset. " + e.GetBaseException().Message);
                return;
            }
            finally
            {
                this._busyManager.End(context);
            }

            await this._application.CommandExecutor.Invoke<GetSharedPackageContainersCommand, IList<SharedPackageContainer>>(this, new GetSharedPackageContainersCommand()).ConfigureAwait(false);
        }

        private async void OnReset()
        {
            var context = this._busyManager.Begin(OperationType.ContainerLoading);
            context.Message = "Resetting container...";

            try
            {
                await this._uac.AsAdministrator<IAppxSharedPackageContainerService>().Reset(this._application.ApplicationState.Containers.SelectedContainer?.Name).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                this._interactionService.ShowError("The container could not be reset. " + e.GetBaseException().Message);
            }
            finally
            {
                this._busyManager.End(context);
            }
        }

        private void OnEdit(object sharedPackageContainerName)
        {
            var dialogRequest = new NavigationRequest
            {
                Type = NavigationRequest.EditingType.EditRunning,
                ContainerName = sharedPackageContainerName as string
            };

            this._prismServices.ModuleManager.LoadModule(ModuleNames.Dialogs.Packaging);
            this._prismServices.DialogService.ShowDialog(NavigationPaths.DialogPaths.PackagingSharedPackageContainer, dialogRequest.ToDialogParameters(), _ => { });
        }

        private bool CanRefresh()
        {
            return true;
        }

        private void OnCopy()
        {
            var container = this._application.ApplicationState.Containers.SelectedContainer;
            if (container != null)
            {
                Clipboard.SetText(GetCopyText(container), TextDataFormat.Text);
            }
        }

        private bool CanCopy()
        {
            return this._application.ApplicationState.Containers.SelectedContainer != null;
        }

        private bool CanEdit(object sharedPackageContainerName)
        {
            return this._application.ApplicationState.Containers.SelectedContainer != null;
        }

        private bool CanReset()
        {
            return this._application.ApplicationState.Containers.SelectedContainer != null;
        }

        private bool CanDelete()
        {
            return this._application.ApplicationState.Containers.SelectedContainer != null;
        }

        private bool CanAdd() => true;

        private static string GetCopyText(SharedPackageContainer container)
        {
            var copiedText = new StringBuilder();

            if (container.Name != default)
            {
                copiedText.Append("[" + container.Name + "]");
            }

            if (container.PackageFamilies?.Any() == true)
            {
                foreach (var fn in container.PackageFamilies.Select(p => p.FamilyName))
                {
                    if (string.IsNullOrWhiteSpace(fn))
                    {
                        continue;
                    }

                    copiedText.AppendFormat(Environment.NewLine + " * " + fn);
                }
            }
            
            return copiedText.ToString().TrimEnd();
        }
    }
}
