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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Data;
using Otor.MsixHero.App.Converters;
using Otor.MsixHero.App.Helpers;
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Hero.Commands.Packages;
using Otor.MsixHero.App.Hero.Events.Base;
using Otor.MsixHero.App.Hero.Executor;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.App.Mvvm.Progress;
using Otor.MsixHero.Appx.Diagnostic.RunningDetector;
using Otor.MsixHero.Appx.Packaging.Installation;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;
using Otor.MsixHero.Appx.Packaging.Manifest.Enums;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Localization;
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
        private readonly HashSet<string> _selectedManifests = new HashSet<string>(StringComparer.Ordinal);
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
            this.Items = new ObservableCollection<SelectableInstalledPackageViewModel>();
            this.ItemsCollection = CollectionViewSource.GetDefaultView(this.Items);
            this.ItemsCollection.Filter = row => this.IsPackageVisible((SelectableInstalledPackageViewModel)row);

            // reloading packages
            this._application.EventAggregator.GetEvent<UiExecutedEvent<GetPackagesCommand, IList<InstalledPackage>>>().Subscribe(this.OnGetPackagesExecuted, ThreadOption.UIThread);

            // selecting packages
            this._application.EventAggregator.GetEvent<UiExecutedEvent<SelectPackagesCommand>>().Subscribe(this.OnSelectPackagesExecuted);

            // starring and unstarring
            this._application.EventAggregator.GetEvent<UiExecutedEvent<StarPackageCommand>>().Subscribe(this.OnStarPackageExecuted, ThreadOption.UIThread);
            this._application.EventAggregator.GetEvent<UiExecutedEvent<UnstarPackageCommand>>().Subscribe(this.OnUnstarPackageExecuted, ThreadOption.UIThread);

            // filtering
            this._application.EventAggregator.GetEvent<UiExecutedEvent<SetPackageFilterCommand>>().Subscribe(this.OnSetPackageFilterCommand, ThreadOption.UIThread);

            // sorting ang grouping
            this._application.EventAggregator.GetEvent<UiExecutedEvent<SetPackageSortingCommand>>().Subscribe(this.OnSetPackageSorting, ThreadOption.UIThread);
            this._application.EventAggregator.GetEvent<UiExecutedEvent<SetPackageGroupingCommand>>().Subscribe(this.OnSetPackageGrouping, ThreadOption.UIThread);

            // is running indicator
            this._application.EventAggregator.GetEvent<PubSubEvent<ActivePackageFullNames>>().Subscribe(this.OnActivePackageIndication);
            this._application.EventAggregator.GetEvent<PubSubEvent<ActivePackageFullNames>>().Subscribe(this.OnActivePackageIndicationFinished, ThreadOption.UIThread);

            this._busyManager.StatusChanged += BusyManagerOnStatusChanged;
            this.SetSortingAndGrouping();

            MsixHeroTranslation.Instance.CultureChanged += (_, _) =>
            {
                var current = this.Items.Where(item => item.IsSelected).ToArray();
                if (current.Length == 1)
                {
                    // Re-select the current package so that we get a refreshed view with all translated pieces.
                    current.First().IsSelected = false;
                    current.First().IsSelected = true;
                }
            };
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

        public ICollectionView ItemsCollection { get; }

        public ObservableCollection<SelectableInstalledPackageViewModel> Items { get; }
        
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
            this.OnPropertyChanged(nameof(SearchKey));
            this.ItemsCollection.Refresh();
        }

        private void OnStarPackageExecuted(UiExecutedPayload<StarPackageCommand> obj)
        {
            var findPackage = this.Items.FirstOrDefault(item => item.PackageFullName == obj.Request.FullName);
            if (findPackage == null)
            {
                return;
            }

            if (!findPackage.HasStar)
            {
                findPackage.HasStar = true;
                return;
            }

            this.ItemsCollection.Refresh();
        }

        private void OnUnstarPackageExecuted(UiExecutedPayload<UnstarPackageCommand> obj)
        {
            var findPackage = this.Items.FirstOrDefault(item => item.PackageFullName == obj.Request.FullName);
            if (findPackage == null)
            {
                return;
            }

            if (findPackage.HasStar)
            {
                findPackage.HasStar = false;
                return;
            }

            this.ItemsCollection.Refresh();
        }

        private void OnSelectPackagesExecuted(UiExecutedPayload<SelectPackagesCommand> obj)
        {
            this._selectedManifests.Clear();
            
            foreach (var item in this._application.ApplicationState.Packages.SelectedPackages.Select(p => p.PackageFullName))
            {
                this._selectedManifests.Add(item);
            }
            
            foreach (var package in this.Items)
            {
                package.IsSelected = this._selectedManifests.Contains(package.PackageFullName);
            }
        }
        
        private void OnGetPackagesExecuted(UiExecutedPayload<GetPackagesCommand, IList<InstalledPackage>> eventPayload)
        {
            this._packagesSync.EnterWriteLock();

            try
            {
                var options = this._application.ConfigurationService.GetCurrentConfiguration();
                this.Items.Clear();

                var starCalculator = new PackageStarHelper(options);
                
                foreach (var item in eventPayload.Result)
                {
                    var isStarred = starCalculator.IsStarred(item.Publisher, item.Name, item.Version, item.Architecture, item.ResourceId);
                    this.Items.Add(new SelectableInstalledPackageViewModel(item, this._commandExecutor, _selectedManifests.Contains(item.PackageFullName), isStarred));
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
                            item.IsSelected = false;
                            return false;
                        }

                        break;
                    case SignatureKind.Store:
                        if ((packageFilterSignatureFlags & PackageFilter.Store) == 0)
                        {
                            item.IsSelected = false;
                            return false;
                        }

                        break;
                    case SignatureKind.System:
                        if ((packageFilterSignatureFlags & PackageFilter.System) == 0)
                        {
                            item.IsSelected = false;
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
                        item.IsSelected = false;
                        return false;
                    }
                }
                else
                {
                    if ((packageFilterAddOnFlags & PackageFilter.MainApps) == 0)
                    {
                        item.IsSelected = false;
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
                                item.IsSelected = false;
                                return false;
                            }

                            break;
                        }
                    case AppxPackageArchitecture.x64:
                        {
                            if ((packageFilterPlatformFlags & PackageFilter.x64) == 0)
                            {
                                item.IsSelected = false;
                                return false;
                            }

                            break;
                        }
                    case AppxPackageArchitecture.Arm:
                        {
                            if ((packageFilterPlatformFlags & PackageFilter.Arm) == 0)
                            {
                                item.IsSelected = false;
                                return false;
                            }

                            break;
                        }
                    case AppxPackageArchitecture.Arm64:
                        {
                            if ((packageFilterPlatformFlags & PackageFilter.Arm64) == 0)
                            {
                                item.IsSelected = false;
                                return false;
                            }

                            break;
                        }
                    case AppxPackageArchitecture.Neutral:
                        {
                            if ((packageFilterPlatformFlags & PackageFilter.Neutral) == 0)
                            {
                                item.IsSelected = false;
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
                    item.IsSelected = false;
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
                item.IsSelected = false;
                return false;
            }

            return true;
        }

        private void OnActivePackageIndication(ActivePackageFullNames obj)
        {
            this._packagesSync.EnterReadLock();
            try
            {
                foreach (var item in this.Items)
                {
                    item.IsRunning = obj.Running.Contains(item.PackageFamilyName);
                }
            }
            finally
            {
                this._packagesSync.ExitReadLock();
            }
        }

        private void OnActivePackageIndicationFinished(ActivePackageFullNames obj)
        {
            this.ItemsCollection.Refresh();
        }

        private void OnSetPackageGrouping(UiExecutedPayload<SetPackageGroupingCommand> obj)
        {
            this.SetSortingAndGrouping();
        }

        private void OnSetPackageSorting(UiExecutedPayload<SetPackageSortingCommand> obj)
        {
            this.SetSortingAndGrouping();
        }

        private void SetSortingAndGrouping()
        {
            var currentSort = this._application.ApplicationState.Packages.SortMode;
            var currentSortDescending = this._application.ApplicationState.Packages.SortDescending;
            var currentGroup = this._application.ApplicationState.Packages.GroupMode;

            using (this.ItemsCollection.DeferRefresh())
            {
                string sortProperty;
                string groupProperty;

                switch (currentSort)
                {
                    case PackageSort.Name:
                        sortProperty = nameof(SelectableInstalledPackageViewModel.DisplayName);
                        break;
                    case PackageSort.Publisher:
                        sortProperty = nameof(SelectableInstalledPackageViewModel.DisplayPublisherName);
                        break;
                    case PackageSort.Architecture:
                        sortProperty = nameof(SelectableInstalledPackageViewModel.Architecture);
                        break;
                    case PackageSort.InstallDate:
                        sortProperty = nameof(SelectableInstalledPackageViewModel.InstallDate);
                        break;
                    case PackageSort.Type:
                        sortProperty = nameof(SelectableInstalledPackageViewModel.Type);
                        break;
                    case PackageSort.Version:
                        sortProperty = nameof(SelectableInstalledPackageViewModel.Version);
                        break;
                    case PackageSort.PackageType:
                        sortProperty = nameof(SelectableInstalledPackageViewModel.DisplayPackageType);
                        break;
                    default:
                        sortProperty = null;
                        break;
                }

                switch (currentGroup)
                {
                    case PackageGroup.None:
                        groupProperty = null;
                        break;
                    case PackageGroup.Publisher:
                        groupProperty = nameof(SelectableInstalledPackageViewModel.DisplayPublisherName);
                        break;
                    case PackageGroup.Architecture:
                        groupProperty = nameof(SelectableInstalledPackageViewModel.Architecture);
                        break;
                    case PackageGroup.InstallDate:
                        groupProperty = nameof(SelectableInstalledPackageViewModel.InstallDate);
                        break;
                    case PackageGroup.Type:
                        groupProperty = nameof(SelectableInstalledPackageViewModel.Type);
                        break;
                    default:
                        return;
                }

                // 1) First grouping
                if (groupProperty == null)
                {
                    this.ItemsCollection.GroupDescriptions.Clear();
                }
                else
                {
                    var pgd = this.ItemsCollection.GroupDescriptions.OfType<PropertyGroupDescription>().FirstOrDefault();
                    if (pgd == null || pgd.PropertyName != groupProperty)
                    {
                        this.ItemsCollection.GroupDescriptions.Clear();

                        if (groupProperty == nameof(SelectableInstalledPackageViewModel.InstallDate))
                        {
                            this.ItemsCollection.GroupDescriptions.Add(new PropertyGroupDescription(groupProperty, GroupDateConverter.Instance));
                        }
                        else
                        {
                            this.ItemsCollection.GroupDescriptions.Add(new PropertyGroupDescription(groupProperty));
                        }
                    }
                }

                // 2) Then sorting
                if (sortProperty == null)
                {
                    this.ItemsCollection.SortDescriptions.Clear();
                    this.ItemsCollection.SortDescriptions.Add(new SortDescription(nameof(SelectableInstalledPackageViewModel.HasStar), ListSortDirection.Descending));
                }
                else
                {
                    this.ItemsCollection.SortDescriptions.Clear();
                    this.ItemsCollection.SortDescriptions.Add(new SortDescription(nameof(SelectableInstalledPackageViewModel.HasStar), ListSortDirection.Descending));
                    this.ItemsCollection.SortDescriptions.Add(new SortDescription(sortProperty, currentSortDescending ? ListSortDirection.Descending : ListSortDirection.Ascending));
                }

                if (this.ItemsCollection.GroupDescriptions.Any())
                {
                    var gpn = ((PropertyGroupDescription)this.ItemsCollection.GroupDescriptions[0]).PropertyName;
                    if (this.ItemsCollection.GroupDescriptions.Any() && this.ItemsCollection.SortDescriptions.All(sd => sd.PropertyName != gpn))
                    {
                        this.ItemsCollection.SortDescriptions.Insert(0, new SortDescription(gpn, ListSortDirection.Ascending));
                    }
                }
            }
        }
    }
}
