using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using Otor.MsixHero.App.Helpers;
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Hero.Commands.Packages;
using Otor.MsixHero.App.Hero.Events.Base;
using Otor.MsixHero.App.Hero.Executor;
using Otor.MsixHero.App.Modules.Packages.ViewModels.Items;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Packaging.Installation;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Lib.Infrastructure.Progress;
using Prism;
using Prism.Events;
using Prism.Regions;

namespace Otor.MsixHero.App.Modules.Packages.ViewModels
{
    public class PackagesListViewModel : NotifyPropertyChanged, INavigationAware, IActiveAware
    {
        private readonly IBusyManager busyManager;
        private readonly IMsixHeroApplication application;
        private readonly IInteractionService interactionService;
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

            this.application.EventAggregator.GetEvent<UiExecutingEvent<GetPackagesCommand>>().Subscribe(this.OnGetPackagesExecuting);
            this.application.EventAggregator.GetEvent<UiExecutedEvent<GetPackagesCommand, IList<InstalledPackage>>>().Subscribe(this.OnGetPackagesExecuted, ThreadOption.UIThread);
            this.application.EventAggregator.GetEvent<UiFailedEvent<GetPackagesCommand>>().Subscribe(this.OnGetPackagesFailed);
            this.application.EventAggregator.GetEvent<UiCancelledEvent<GetPackagesCommand>>().Subscribe(this.OnGetPackagesCancelled);
            this.application.EventAggregator.GetEvent<UiExecutedEvent<SetPackageFilterCommand>>().Subscribe(this.OnSetPackageFilterCommand, ThreadOption.UIThread);

            this.busyManager.StatusChanged += BusyManagerOnStatusChanged;
        }

        public string SearchKey
        {
            get => this.application.ApplicationState.Packages.SearchKey;
            set => this.application.CommandExecutor.Invoke(this, new SetPackageFilterCommand(this.application.CommandExecutor.ApplicationState.Packages.PackageFilter, value));
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

                this.IsActiveChanged?.Invoke(this, new EventArgs());

                if (firstRun)
                {
                    this.application
                        .CommandExecutor
                        .WithBusyManager(this.busyManager, OperationType.PackageLoading)
                        .WithErrorHandling(this.interactionService, true)
                        .Invoke(this, new GetPackagesCommand(PackageFindMode.Auto));
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
            this.Items.Clear();

            foreach (var item in eventPayload.Result)
            {
                this.Items.Add(new InstalledPackageViewModel(item));
            }
        }
        private bool IsPackageVisible(InstalledPackageViewModel item)
        {
            var signatureFlags = this.application.ApplicationState.Packages.PackageFilter & PackageFilter.AllSources;
            if (signatureFlags != 0 && signatureFlags != PackageFilter.AllSources)
            {
                switch (item.SignatureKind)
                {
                    case SignatureKind.Developer:
                    case SignatureKind.Unsigned:
                    case SignatureKind.Enterprise:
                        if ((signatureFlags & PackageFilter.Developer) == 0)
                        {
                            return false;
                        }

                        break;
                    case SignatureKind.Store:
                        if ((signatureFlags & PackageFilter.Store) == 0)
                        {
                            return false;
                        }

                        break;
                    case SignatureKind.System:
                        if ((signatureFlags & PackageFilter.System) == 0)
                        {
                            return false;
                        }

                        break;
                }
            }

            var addonFlags = this.application.ApplicationState.Packages.PackageFilter & PackageFilter.MainAppsAndAddOns;
            if (addonFlags != 0 && addonFlags != PackageFilter.MainAppsAndAddOns)
            {
                if (item.IsAddon)
                {
                    if ((addonFlags & PackageFilter.Addons) == 0)
                    {
                        return false;
                    }
                }
                else
                {
                    if ((addonFlags & PackageFilter.MainApps) == 0)
                    {
                        return false;
                    }
                }
            }

            var archFilter = this.application.ApplicationState.Packages.PackageFilter & PackageFilter.AllArchitectures;
            if (archFilter != PackageFilter.AllArchitectures && archFilter != 0)
            {
                switch (item.Model.Architecture?.ToLowerInvariant())
                {
                    case "x86":
                        {
                            if ((archFilter & PackageFilter.x86) == 0)
                            {
                                return false;
                            }

                            break;
                        }
                    case "x64":
                        {
                            if ((archFilter & PackageFilter.x64) == 0)
                            {
                                return false;
                            }

                            break;
                        }
                    case "arm":
                        {
                            if ((archFilter & PackageFilter.Arm) == 0)
                            {
                                return false;
                            }

                            break;
                        }
                    case "arm64":
                        {
                            if ((archFilter & PackageFilter.Arm64) == 0)
                            {
                                return false;
                            }

                            break;
                        }
                    case "neutral":
                        {
                            if ((archFilter & PackageFilter.Neutral) == 0)
                            {
                                return false;
                            }

                            break;
                        }
                }
            }

            var isRunningFilter = this.application.ApplicationState.Packages.PackageFilter & PackageFilter.InstalledAndRunning;
            if (isRunningFilter == PackageFilter.Running)
            {
                if (!item.IsRunning)
                {
                    // return false;
                }
            }

            if (!string.IsNullOrWhiteSpace(this.application.ApplicationState.Packages.SearchKey) && item.DisplayName.IndexOf(this.application.ApplicationState.Packages.SearchKey, StringComparison.OrdinalIgnoreCase) == -1
                   && item.DisplayPublisherName.IndexOf(this.application.ApplicationState.Packages.SearchKey, StringComparison.OrdinalIgnoreCase) == -1
                   && item.Version.IndexOf(this.application.ApplicationState.Packages.SearchKey, StringComparison.OrdinalIgnoreCase) == -1
                   && item.Architecture.IndexOf(this.application.ApplicationState.Packages.SearchKey, StringComparison.OrdinalIgnoreCase) == -1)
            {
                return false;
            }

            return true;
        }
    }
}
