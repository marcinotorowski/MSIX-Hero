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
using Otor.MsixHero.Appx.Diagnostic.RunningDetector;
using Otor.MsixHero.Appx.Packaging.Installation;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Lib.Infrastructure.Progress;
using Prism;
using Prism.Events;
using Prism.Regions;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageList.ViewModels
{
    public class PackagesListViewModel : NotifyPropertyChanged, INavigationAware, IActiveAware
    {
        private readonly IBusyManager busyManager;
        private readonly IMsixHeroApplication application;
        private readonly IInteractionService interactionService;
        private readonly ReaderWriterLockSlim packagesSync = new ReaderWriterLockSlim();
        private bool firstRun = true;
        private bool isActive;

        public PackagesListViewModel(
            IBusyManager busyManager,
            IMsixHeroApplication application,
            IInteractionService interactionService)
        {
            this.busyManager = busyManager;
            this.application = application;
            this.interactionService = interactionService;
            this.Items = new ObservableCollection<InstalledPackageViewModel>();
            this.ItemsCollection = CollectionViewSource.GetDefaultView(this.Items);
            this.ItemsCollection.Filter = row => this.IsPackageVisible((InstalledPackageViewModel)row);

            // reloading packages
            this.application.EventAggregator.GetEvent<UiExecutingEvent<GetPackagesCommand>>().Subscribe(this.OnGetPackagesExecuting);
            this.application.EventAggregator.GetEvent<UiExecutedEvent<GetPackagesCommand, IList<InstalledPackage>>>().Subscribe(this.OnGetPackagesExecuted, ThreadOption.UIThread);
            this.application.EventAggregator.GetEvent<UiFailedEvent<GetPackagesCommand>>().Subscribe(this.OnGetPackagesFailed);
            this.application.EventAggregator.GetEvent<UiCancelledEvent<GetPackagesCommand>>().Subscribe(this.OnGetPackagesCancelled);

            // filtering
            this.application.EventAggregator.GetEvent<UiExecutedEvent<SetPackageFilterCommand>>().Subscribe(this.OnSetPackageFilterCommand, ThreadOption.UIThread);

            // sorting ang grouping
            this.application.EventAggregator.GetEvent<UiExecutedEvent<SetPackageSortingCommand>>().Subscribe(this.OnSetPackageSorting, ThreadOption.UIThread);
            this.application.EventAggregator.GetEvent<UiExecutedEvent<SetPackageGroupingCommand>>().Subscribe(this.OnSetPackageGrouping, ThreadOption.UIThread);

            // is running indicator
            this.application.EventAggregator.GetEvent<PubSubEvent<ActivePackageFullNames>>().Subscribe(this.OnActivePackageIndication);
            this.application.EventAggregator.GetEvent<PubSubEvent<ActivePackageFullNames>>().Subscribe(this.OnActivePackageIndicationFinished, ThreadOption.UIThread);

            this.busyManager.StatusChanged += BusyManagerOnStatusChanged;
            this.SetSortingAndGrouping();
        }

        public string SearchKey
        {
            get => this.application.ApplicationState.Packages.SearchKey;
            set => this.application.CommandExecutor.Invoke(this, new SetPackageFilterCommand(this.application.CommandExecutor.ApplicationState.Packages.Filter, value));
        }

        public bool IsActive
        {
            get => this.isActive;
            set
            {
                if (!this.SetField(ref this.isActive, value))
                {
                    return;
                }

                this.IsActiveChanged?.Invoke(this, EventArgs.Empty);

                if (firstRun)
                {
                    this.application
                        .CommandExecutor
                        .WithBusyManager(this.busyManager, OperationType.PackageLoading)
                        .WithErrorHandling(this.interactionService, true)
                        .Invoke<GetPackagesCommand, IList<InstalledPackage>>(this, new GetPackagesCommand(PackageFindMode.Auto));
                }

                this.firstRun = false;
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

        public ObservableCollection<InstalledPackageViewModel> Items { get; }

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

        private void OnGetPackagesExecuting(UiExecutingPayload<GetPackagesCommand> eventPayload)
        {
        }

        private void OnGetPackagesCancelled(UiCancelledPayload<GetPackagesCommand> eventPayload)
        {
        }

        private void OnGetPackagesFailed(UiFailedPayload<GetPackagesCommand> eventPayload)
        {
        }

        private void OnSetPackageFilterCommand(UiExecutedPayload<SetPackageFilterCommand> obj)
        {
            this.OnPropertyChanged(nameof(SearchKey));
            this.ItemsCollection.Refresh();
        }

        private void OnGetPackagesExecuted(UiExecutedPayload<GetPackagesCommand, IList<InstalledPackage>> eventPayload)
        {
            this.packagesSync.EnterWriteLock();

            try
            {
                this.Items.Clear();

                foreach (var item in eventPayload.Result)
                {
                    this.Items.Add(new InstalledPackageViewModel(item));
                }
            }
            finally
            {
                this.packagesSync.ExitWriteLock();
            }
        }

        private bool IsPackageVisible(InstalledPackageViewModel item)
        {
            var packageFilterSignatureFlags = this.application.ApplicationState.Packages.Filter & PackageFilter.AllSources;
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

            var packageFilterAddOnFlags = this.application.ApplicationState.Packages.Filter & PackageFilter.MainAppsAndAddOns;
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

            var packageFilterPlatformFlags = this.application.ApplicationState.Packages.Filter & PackageFilter.AllArchitectures;
            if (packageFilterPlatformFlags != PackageFilter.AllArchitectures && packageFilterPlatformFlags != 0)
            {
                switch (item.Model.Architecture?.ToLowerInvariant())
                {
                    case "x86":
                        {
                            if ((packageFilterPlatformFlags & PackageFilter.x86) == 0)
                            {
                                return false;
                            }

                            break;
                        }
                    case "x64":
                        {
                            if ((packageFilterPlatformFlags & PackageFilter.x64) == 0)
                            {
                                return false;
                            }

                            break;
                        }
                    case "arm":
                        {
                            if ((packageFilterPlatformFlags & PackageFilter.Arm) == 0)
                            {
                                return false;
                            }

                            break;
                        }
                    case "arm64":
                        {
                            if ((packageFilterPlatformFlags & PackageFilter.Arm64) == 0)
                            {
                                return false;
                            }

                            break;
                        }
                    case "neutral":
                        {
                            if ((packageFilterPlatformFlags & PackageFilter.Neutral) == 0)
                            {
                                return false;
                            }

                            break;
                        }
                }
            }

            var packageFilterIsRunningFlags = this.application.ApplicationState.Packages.Filter & PackageFilter.InstalledAndRunning;
            if (packageFilterIsRunningFlags == PackageFilter.Running)
            {
                if (!item.IsRunning)
                {
                    return false;
                }
            }

            // If the user searched nothing, return all packages
            if (string.IsNullOrWhiteSpace(this.application.ApplicationState.Packages.SearchKey))
            {
                return true;
            }

            if (
                (item.DisplayName?.IndexOf(this.application.ApplicationState.Packages.SearchKey, StringComparison.OrdinalIgnoreCase) ?? -1) == -1 && 
                (item.DisplayPublisherName?.IndexOf(this.application.ApplicationState.Packages.SearchKey, StringComparison.OrdinalIgnoreCase) ?? -1) == -1 && 
                (item.ProductId?.IndexOf(this.application.ApplicationState.Packages.SearchKey, StringComparison.OrdinalIgnoreCase) ?? -1) == -1 &&
                (item.PackageFamilyName?.IndexOf(this.application.ApplicationState.Packages.SearchKey, StringComparison.OrdinalIgnoreCase) ?? -1) == -1
            )
            {
                return false;
            }

            return true;
        }

        private void OnActivePackageIndication(ActivePackageFullNames obj)
        {
            this.packagesSync.EnterReadLock();
            try
            {
                foreach (var item in this.Items)
                {
                    item.IsRunning = obj.Running.Contains(item.PackageFamilyName);
                }
            }
            finally
            {
                this.packagesSync.ExitReadLock();
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
            var currentSort = this.application.ApplicationState.Packages.SortMode;
            var currentSortDescending = this.application.ApplicationState.Packages.SortDescending;
            var currentGroup = this.application.ApplicationState.Packages.GroupMode;

            using (this.ItemsCollection.DeferRefresh())
            {
                string sortProperty;
                string groupProperty;

                switch (currentSort)
                {
                    case PackageSort.Name:
                        sortProperty = nameof(InstalledPackageViewModel.DisplayName);
                        break;
                    case PackageSort.Publisher:
                        sortProperty = nameof(InstalledPackageViewModel.DisplayPublisherName);
                        break;
                    case PackageSort.Architecture:
                        sortProperty = nameof(InstalledPackageViewModel.Architecture);
                        break;
                    case PackageSort.InstallDate:
                        sortProperty = nameof(InstalledPackageViewModel.InstallDate);
                        break;
                    case PackageSort.Type:
                        sortProperty = nameof(InstalledPackageViewModel.Type);
                        break;
                    case PackageSort.Version:
                        sortProperty = nameof(InstalledPackageViewModel.Version);
                        break;
                    case PackageSort.PackageType:
                        sortProperty = nameof(InstalledPackageViewModel.DisplayPackageType);
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
                        groupProperty = nameof(InstalledPackageViewModel.DisplayPublisherName);
                        break;
                    case PackageGroup.Architecture:
                        groupProperty = nameof(InstalledPackageViewModel.Architecture);
                        break;
                    case PackageGroup.InstallDate:
                        groupProperty = nameof(InstalledPackageViewModel.InstallDate);
                        break;
                    case PackageGroup.Type:
                        groupProperty = nameof(InstalledPackageViewModel.Type);
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

                        if (groupProperty == nameof(InstalledPackageViewModel.InstallDate))
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
                }
                else
                {
                    var sd = this.ItemsCollection.SortDescriptions.FirstOrDefault();
                    if (sd.PropertyName != sortProperty || sd.Direction != (currentSortDescending ? ListSortDirection.Descending : ListSortDirection.Ascending))
                    {
                        this.ItemsCollection.SortDescriptions.Clear();
                        this.ItemsCollection.SortDescriptions.Add(new SortDescription(sortProperty, currentSortDescending ? ListSortDirection.Descending : ListSortDirection.Ascending));
                    }
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
