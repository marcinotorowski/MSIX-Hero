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
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Hero.Commands.Packages;
using Otor.MsixHero.App.Hero.Events.Base;
using Otor.MsixHero.App.Hero.Executor;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.App.Mvvm.Progress;
using Otor.MsixHero.Appx.Diagnostic.RunningDetector;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;
using Otor.MsixHero.Appx.Packaging.Manifest.Enums;
using Otor.MsixHero.Appx.Packaging.Services;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Services;
using Prism;
using Prism.Events;
using Prism.Regions;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageList.ViewModels
{
    public class PackagesListViewModel : NotifyPropertyChanged, INavigationAware, IActiveAware
    {
        private readonly IBusyManager _busyManager;
        private readonly IMsixHeroApplication _application;
        private readonly IMsixHeroCommandExecutor _commandExecutor;
        private readonly IInteractionService _interactionService;
        private readonly ReaderWriterLockSlim _packagesSync = new ReaderWriterLockSlim();
        private bool _firstRun = true;
        private bool _isActive;

        public PackagesListViewModel(
            IBusyManager busyManager,
            IMsixHeroApplication application,
            IMsixHeroCommandExecutor commandExecutor,
            IInteractionService interactionService)
        {
            this._busyManager = busyManager;
            this._application = application;
            _commandExecutor = commandExecutor;
            this._interactionService = interactionService;

            // reloading packages
            this._application.EventAggregator.GetEvent<UiExecutedEvent<GetPackagesCommand, IList<InstalledPackage>>>().Subscribe(this.OnGetPackagesExecuted, ThreadOption.UIThread);

            // selecting packages
            this._application.EventAggregator.GetEvent<UiExecutedEvent<SelectPackagesCommand>>().Subscribe(this.OnSelectPackagesExecuted);

            // starring and unstarring
            this._application.EventAggregator.GetEvent<UiExecutedEvent<StarPackageCommand>>().Subscribe(this.OnStarPackageExecuted);
            this._application.EventAggregator.GetEvent<UiExecutedEvent<UnstarPackageCommand>>().Subscribe(this.OnUnstarPackageExecuted);

            // filtering
            this._application.EventAggregator.GetEvent<UiExecutedEvent<SetPackageFilterCommand>>().Subscribe(this.OnSetPackageFilterCommand);
            
            // is running indicator
            this._application.EventAggregator.GetEvent<PubSubEvent<ActivePackageFullNames>>().Subscribe(this.OnActivePackageIndication);

            this._busyManager.StatusChanged += BusyManagerOnStatusChanged;
        }

        public string SearchKey
        {
            get => this._application.ApplicationState.Packages.SearchKey;
            set => this._application.CommandExecutor.Invoke(this, new SetPackageFilterCommand(this._application.CommandExecutor.ApplicationState.Packages.Filter, value));
        }

        public bool IsActive
        {
            get => this._isActive;
            set
            {
                if (!this.SetField(ref this._isActive, value))
                {
                    return;
                }

                this.IsActiveChanged?.Invoke(this, EventArgs.Empty);

                if (_firstRun)
                {
                    this._application
                        .CommandExecutor
                        .WithBusyManager(this._busyManager, OperationType.PackageLoading)
                        .WithErrorHandling(this._interactionService, true)
                        .Invoke<GetPackagesCommand, IList<InstalledPackage>>(this, new GetPackagesCommand(PackageFindMode.Auto));
                }

                this._firstRun = false;
            }
        }

        public event EventHandler IsActiveChanged;

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        public ProgressProperty Progress { get; } = new ProgressProperty();

        public IList<SelectableInstalledPackageViewModel> AllPackages { get; } = new List<SelectableInstalledPackageViewModel>();

        public IList<SelectableInstalledPackageViewModel> SelectedPackages { get; } = new List<SelectableInstalledPackageViewModel>();
        
        private void BusyManagerOnStatusChanged(object sender, IBusyStatusChange e)
        {
            if (e.Type != OperationType.PackageLoading && e.Type != OperationType.Other)
            {
                return;
            }

            this.Progress.IsLoading = e.IsBusy;
            this.Progress.Message = e.Message;
            this.Progress.Progress = e.Progress;
        }
        
        private void OnSetPackageFilterCommand(UiExecutedPayload<SetPackageFilterCommand> obj)
        {
            this.RefreshVisibility();
            this.OnPropertyChanged(nameof(SearchKey));
        }
        
        private void RefreshVisibility()
        {
            var toRemove = new List<SelectableInstalledPackageViewModel>();
            
            foreach (var package in this.AllPackages)
            {
                var oldVisibility = package.IsVisible;
                var newVisibility = this.IsPackageVisible(package);
                
                package.IsVisible = newVisibility;
                if (newVisibility || !oldVisibility)
                {
                    continue;
                }

                if (this.SelectedPackages.Remove(package))
                { 
                    toRemove.Add(package);
                }
            }

            if (toRemove.Any())
            {
                // if there was any change, let's inform the main backend that we de-selected something...
                this._commandExecutor.Invoke(this, new SelectPackagesCommand(toRemove.Select(p => p.PackageFullName)));
            }
        }
        
        private void OnStarPackageExecuted(UiExecutedPayload<StarPackageCommand> obj)
        {
            var findPackage = this.AllPackages.FirstOrDefault(item => item.PackageFullName == obj.Request.FullName);
            if (findPackage == null)
            {
                return;
            }

            if (!findPackage.HasStar)
            {
                findPackage.HasStar = true;
            }
        }

        private void OnUnstarPackageExecuted(UiExecutedPayload<UnstarPackageCommand> obj)
        {
            var findPackage = this.AllPackages.FirstOrDefault(item => item.PackageFullName == obj.Request.FullName);
            if (findPackage == null)
            {
                return;
            }

            if (findPackage.HasStar)
            {
                findPackage.HasStar = false;
            }
        }

        private void OnSelectPackagesExecuted(UiExecutedPayload<SelectPackagesCommand> obj)
        {
            var allSelected = new HashSet<string>(this._application.ApplicationState.Packages.SelectedPackages.Select(p => p.PackageFullName));
            
            this.SelectedPackages.Clear();
            
            foreach (var item in this.AllPackages.Where(p => p.IsVisible && allSelected.Contains(p.PackageFullName)))
            {
                this.SelectedPackages.Add(item);
            }
        }
        
        private void OnGetPackagesExecuted(UiExecutedPayload<GetPackagesCommand, IList<InstalledPackage>> eventPayload)
        {
            this._packagesSync.EnterWriteLock();

            try
            {
                var options = this._application.ConfigurationService.GetCurrentConfiguration();
                this.AllPackages.Clear();

                var starCalculator = new PackageStarHelper(options);
                
                foreach (var item in eventPayload.Result)
                {
                    var isStarred = starCalculator.IsStarred(item.Publisher, item.Name, item.Version, item.Architecture, item.ResourceId);
                    var np = new SelectableInstalledPackageViewModel(item, this._commandExecutor, isStarred);
                    np.IsVisible = this.IsPackageVisible(np);
                    this.AllPackages.Add(np);
                }
            }
            finally
            {
                this._packagesSync.ExitWriteLock();
            }
        }

        private bool IsPackageVisible(SelectableInstalledPackageViewModel item)
        {
            var packageFilterSignatureFlags = this._application.ApplicationState.Packages.Filter & PackageFilter.AllSources;
            if (packageFilterSignatureFlags != 0 && packageFilterSignatureFlags != PackageFilter.AllSources)
            {
                switch (item.SignatureKind)
                {
                    case SignatureKind.Developer:
                    case SignatureKind.Unsigned:
                    case SignatureKind.Enterprise:
                        if ((packageFilterSignatureFlags & PackageFilter.Developer) == 0)
                        {
                            return false;
                        }

                        break;
                    case SignatureKind.Store:
                        if ((packageFilterSignatureFlags & PackageFilter.Store) == 0)
                        {
                            return false;
                        }

                        break;
                    case SignatureKind.System:
                        if ((packageFilterSignatureFlags & PackageFilter.System) == 0)
                        {
                            return false;
                        }

                        break;
                }
            }

            var packageFilterAddOnFlags = this._application.ApplicationState.Packages.Filter & PackageFilter.MainAppsAndAddOns;
            if (packageFilterAddOnFlags != 0 && packageFilterAddOnFlags != PackageFilter.MainAppsAndAddOns)
            {
                if (item.IsAddon)
                {
                    if ((packageFilterAddOnFlags & PackageFilter.Addons) == 0)
                    {
                        return false;
                    }
                }
                else
                {
                    if ((packageFilterAddOnFlags & PackageFilter.MainApps) == 0)
                    {
                        return false;
                    }
                }
            }

            var packageFilterPlatformFlags = this._application.ApplicationState.Packages.Filter & PackageFilter.AllArchitectures;
            if (packageFilterPlatformFlags != PackageFilter.AllArchitectures && packageFilterPlatformFlags != 0)
            {
                switch (item.Model.Architecture)
                {
                    case AppxPackageArchitecture.x86:
                        {
                            if ((packageFilterPlatformFlags & PackageFilter.x86) == 0)
                            {
                                return false;
                            }

                            break;
                        }
                    case AppxPackageArchitecture.x64:
                        {
                            if ((packageFilterPlatformFlags & PackageFilter.x64) == 0)
                            {
                                return false;
                            }

                            break;
                        }
                    case AppxPackageArchitecture.Arm:
                        {
                            if ((packageFilterPlatformFlags & PackageFilter.Arm) == 0)
                            {
                                return false;
                            }

                            break;
                        }
                    case AppxPackageArchitecture.Arm64:
                        {
                            if ((packageFilterPlatformFlags & PackageFilter.Arm64) == 0)
                            {
                                return false;
                            }

                            break;
                        }
                    case AppxPackageArchitecture.Neutral:
                        {
                            if ((packageFilterPlatformFlags & PackageFilter.Neutral) == 0)
                            {
                                return false;
                            }

                            break;
                        }
                }
            }

            var packageFilterIsRunningFlags = this._application.ApplicationState.Packages.Filter & PackageFilter.InstalledAndRunning;
            if (packageFilterIsRunningFlags == PackageFilter.Running)
            {
                if (!item.IsRunning)
                {
                    return false;
                }
            }

            // If the user searched nothing, return all packages
            if (string.IsNullOrWhiteSpace(this._application.ApplicationState.Packages.SearchKey))
            {
                return true;
            }

            if (
                (item.DisplayName?.IndexOf(this._application.ApplicationState.Packages.SearchKey, StringComparison.OrdinalIgnoreCase) ?? -1) == -1 && 
                (item.DisplayPublisherName?.IndexOf(this._application.ApplicationState.Packages.SearchKey, StringComparison.OrdinalIgnoreCase) ?? -1) == -1 && 
                (item.PackageFullName?.IndexOf(this._application.ApplicationState.Packages.SearchKey, StringComparison.OrdinalIgnoreCase) ?? -1) == -1 &&
                (item.PackageFamilyName?.IndexOf(this._application.ApplicationState.Packages.SearchKey, StringComparison.OrdinalIgnoreCase) ?? -1) == -1
            )
            {
                return false;
            }

            return true;
        }

        private void OnActivePackageIndication(ActivePackageFullNames obj)
        {
            this._packagesSync.EnterReadLock();
            try
            {
                var anyChange = false;

                foreach (var item in this.AllPackages)
                {
                    var oldRunning = item.IsRunning;
                    var newIsRunning = obj.Running.Contains(item.PackageFamilyName);

                    if (oldRunning == newIsRunning)
                    {
                        continue;
                    }

                    anyChange = true;
                    item.IsRunning = newIsRunning;
                }

                if (anyChange && this._application.ApplicationState.Packages.Filter.HasFlag(PackageFilter.Running))
                {
                    // Refresh visibility if there was at least one change AND the user is currently filtering by the running state.
                    this.RefreshVisibility();
                }
            }
            finally
            {
                this._packagesSync.ExitReadLock();
            }
        }
    }
}
