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

using System.Collections.Generic;
using System.Threading;
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Hero.Commands.Packages;
using Otor.MsixHero.App.Hero.Events.Base;
using Otor.MsixHero.App.Hero.Executor;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.App.Mvvm.Progress;
using Otor.MsixHero.Appx.Common.Enums;
using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Appx.Packaging.Services;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Events;

namespace Otor.MsixHero.App.Modules.PackageManagement.Search.ViewModels
{
    public class PackagesSearchViewModel : NotifyPropertyChanged
    {
        private readonly IMsixHeroApplication application;
        private readonly IBusyManager busyManager;
        private readonly IInteractionService interactionService;
        private bool isAllUsers;

        public PackagesSearchViewModel(
            IEventAggregator eventAggregator,
            IMsixHeroApplication application,
            IBusyManager busyManager,
            IInteractionService interactionService)
        {
            this.application = application;
            this.busyManager = busyManager;
            this.interactionService = interactionService;
            this.application.EventAggregator.GetEvent<UiExecutedEvent<SetPackageFilterCommand>>().Subscribe(this.OnSetPackageFilterCommand);
            this.isAllUsers = application.ApplicationState.Packages.Mode == PackageInstallationContext.AllUsers;

            eventAggregator.GetEvent<UiExecutedEvent<GetInstalledPackagesCommand>>().Subscribe(this.OnGetPackages);
            eventAggregator.GetEvent<UiFailedEvent<GetInstalledPackagesCommand>>().Subscribe(this.OnGetPackages);
            eventAggregator.GetEvent<UiCancelledEvent<GetInstalledPackagesCommand>>().Subscribe(this.OnGetPackages);
        }
        
        public string SearchKey
        {
            get => this.application.ApplicationState.Packages.SearchKey;
            set => this.application.CommandExecutor.Invoke(this, new SetPackageFilterCommand(this.application.CommandExecutor.ApplicationState.Packages.Filter, value));
        }

        public bool IsAllUsers
        {
            get => this.isAllUsers;
            set
            {
                if (!this.SetField(ref this.isAllUsers, value))
                {
                    return;
                }

                this.LoadContext(value ? PackageFindMode.AllUsers : PackageFindMode.CurrentUser);
            }
        }
        
        private void OnGetPackages(UiFailedPayload<GetInstalledPackagesCommand> obj)
        {
            this.isAllUsers = this.application.ApplicationState.Packages.Mode == PackageInstallationContext.AllUsers;
            this.OnPropertyChanged(nameof(IsAllUsers));
        }

        private void OnGetPackages(UiExecutedPayload<GetInstalledPackagesCommand> obj)
        {
            this.isAllUsers = this.application.ApplicationState.Packages.Mode == PackageInstallationContext.AllUsers;
            this.OnPropertyChanged(nameof(IsAllUsers));
        }

        private void OnGetPackages(UiCancelledPayload<GetInstalledPackagesCommand> obj)
        {
            this.isAllUsers = this.application.ApplicationState.Packages.Mode == PackageInstallationContext.AllUsers;
            this.OnPropertyChanged(nameof(IsAllUsers));
        }

        private async void LoadContext(PackageFindMode mode)
        {
            var executor = this.application.CommandExecutor
                .WithBusyManager(this.busyManager, OperationType.PackageLoading)
                .WithErrorHandling(this.interactionService, true);

            await executor.Invoke<GetInstalledPackagesCommand, IList<PackageEntry>>(this, new GetInstalledPackagesCommand(mode), CancellationToken.None).ConfigureAwait(false);
        }

        private void OnSetPackageFilterCommand(UiExecutedPayload<SetPackageFilterCommand> obj)
        {
            this.OnPropertyChanged(nameof(SearchKey));
        }
    }
}
