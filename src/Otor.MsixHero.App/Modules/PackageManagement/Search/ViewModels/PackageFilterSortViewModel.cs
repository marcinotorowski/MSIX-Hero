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
using System.Windows.Input;
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Hero.Commands.Packages;
using Otor.MsixHero.App.Hero.Events.Base;
using Otor.MsixHero.App.Hero.Executor;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.App.Mvvm.Progress;
using Otor.MsixHero.Appx.Common.Enums;
using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Appx.Packaging.Services;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Localization;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Commands;

namespace Otor.MsixHero.App.Modules.PackageManagement.Search.ViewModels
{
    public enum ClearFilter
    {
        Architecture,
        Activity,
        Category,
        Type,
        AppInstaller
    }

    public class PackageFilterSortViewModel : NotifyPropertyChanged
    {
        private readonly IMsixHeroApplication _application;
        private readonly IInteractionService _interactionService;
        private readonly IBusyManager _busyManager;
        private PackageInstallationContext _source;
        private bool _isBusy;
        private int _progress;

        public PackageFilterSortViewModel(
            IMsixHeroApplication application, 
            IInteractionService interactionService,
            IBusyManager busyManager)
        {
            this._application = application;
            this._interactionService = interactionService;
            this._busyManager = busyManager;

            this._source = this._application.ApplicationState.Packages.Mode;

            this._application.EventAggregator.GetEvent<UiExecutedEvent<SetPackageFilterCommand>>().Subscribe(this.OnSetPackageFilter);
            this._application.EventAggregator.GetEvent<UiExecutedEvent<SetPackageSortingCommand>>().Subscribe(this.OnSetPackageSorting);
            this._application.EventAggregator.GetEvent<UiExecutedEvent<SetPackageGroupingCommand>>().Subscribe(this.OnSetPackageGrouping);

            this._application.EventAggregator.GetEvent<UiExecutedEvent<GetInstalledPackagesCommand>>().Subscribe(this.OnGetPackages);
            this._application.EventAggregator.GetEvent<UiFailedEvent<GetInstalledPackagesCommand>>().Subscribe(this.OnGetPackages);
            this._application.EventAggregator.GetEvent<UiCancelledEvent<GetInstalledPackagesCommand>>().Subscribe(this.OnGetPackages);

            this._busyManager.StatusChanged += this.BusyManagerOnStatusChanged;

            MsixHeroTranslation.Instance.CultureChanged += (_, _) =>
            {
                this.OnPropertyChanged(nameof(FilterArchitectureCaption));
                this.OnPropertyChanged(nameof(FilterCategoryCaption));
                this.OnPropertyChanged(nameof(FilterTypeCaption));
            };

            this.Clear = new DelegateCommand<object>(this.OnClearFilter);
        }

        public bool IsBusy
        {
            get => this._isBusy;
            private set => this.SetField(ref this._isBusy, value);
        }

        public int Progress
        {
            get => this._progress;
            private set => this.SetField(ref this._progress, value);
        }

        public ICommand Clear { get; }

        public bool IsDescending
        {
            get => this._application.ApplicationState.Packages.SortDescending;
            set => this._application.CommandExecutor.Invoke(this, new SetPackageSortingCommand(this.Sort, value));
        }

        public PackageSort Sort
        {
            get => this._application.ApplicationState.Packages.SortMode;
            set => this._application.CommandExecutor.Invoke(this, new SetPackageSortingCommand(value, this.IsDescending));
        }

        public PackageInstallationContext Source
        {
            get => this._source;
            set => this.LoadContext(value);
        }
        public PackageGroup Group
        {
            get => this._application.ApplicationState.Packages.GroupMode;
            set => this._application.CommandExecutor.Invoke(this, new SetPackageGroupingCommand(value));
        }

        public bool FilterStore
        {
            get => this._application.ApplicationState.Packages.Filter.HasFlag(PackageFilter.Store);
            set => this.SetPackageFilter(PackageFilter.Store, value);
        }

        public bool FilterAddOn
        {
            get => this._application.ApplicationState.Packages.Filter.HasFlag(PackageFilter.Addons);
            set => this.SetPackageFilter(PackageFilter.Addons, value);
        }

        public bool FilterMainApp
        {
            get => this._application.ApplicationState.Packages.Filter.HasFlag(PackageFilter.MainApps);
            set => this.SetPackageFilter(PackageFilter.MainApps, value);
        }

        public bool FilterSideLoaded
        {
            get => this._application.ApplicationState.Packages.Filter.HasFlag(PackageFilter.Developer);
            set => this.SetPackageFilter(PackageFilter.Developer, value);
        }

        public bool FilterSystem
        {
            get => this._application.ApplicationState.Packages.Filter.HasFlag(PackageFilter.System);
            set => this.SetPackageFilter(PackageFilter.System, value);
        }

        public bool FilterX64
        {
            get => this._application.ApplicationState.Packages.Filter.HasFlag(PackageFilter.x64);
            set => this.SetPackageFilter(PackageFilter.x64, value);
        }

        public bool FilterX86
        {
            get => this._application.ApplicationState.Packages.Filter.HasFlag(PackageFilter.x86);
            set => this.SetPackageFilter(PackageFilter.x86, value);
        }

        public bool FilterArm
        {
            get => this._application.ApplicationState.Packages.Filter.HasFlag(PackageFilter.Arm);
            set => this.SetPackageFilter(PackageFilter.Arm, value);
        }

        public bool FilterArm64
        {
            get => this._application.ApplicationState.Packages.Filter.HasFlag(PackageFilter.Arm64);
            set => this.SetPackageFilter(PackageFilter.Arm64, value);
        }

        public bool FilterNeutral
        {
            get => this._application.ApplicationState.Packages.Filter.HasFlag(PackageFilter.Neutral);
            set => this.SetPackageFilter(PackageFilter.Neutral, value);
        }

        public string FilterCategoryCaption
        {
            get
            {
                var val = this._application.ApplicationState.Packages.Filter & PackageFilter.AllSources;
                if (val == 0 || val == PackageFilter.AllSources)
                {
                    return Resources.Localization.FilterAll;
                }

                var selected = 0;
                if (this.FilterSideLoaded)
                {
                    selected++;
                }

                if (this.FilterStore)
                {
                    selected++;
                }

                if (this.FilterSystem)
                {
                    selected++;
                }

                return $"({selected}/3)";
            }
        }

        public string FilterTypeCaption
        {
            get
            {
                var val = this._application.ApplicationState.Packages.Filter & PackageFilter.MainAppsAndAddOns;
                if (val == 0 || val == PackageFilter.MainAppsAndAddOns)
                {
                    return Resources.Localization.FilterAll;
                }

                var selected = 0;
                if (this.FilterMainApp)
                {
                    selected++;
                }

                if (this.FilterAddOn)
                {
                    selected++;
                }
                
                return $"({selected}/2)";
            }
        }

        public string FilterArchitectureCaption
        {
            get
            {
                var val = this._application.ApplicationState.Packages.Filter & PackageFilter.AllArchitectures;
                if (val == 0 || val == PackageFilter.AllArchitectures)
                {
                    return Resources.Localization.FilterAll;
                }

                var selected = 0;
                if (this.FilterX64)
                {
                    selected++;
                }

                if (this.FilterX86)
                {
                    selected++;
                }

                if (this.FilterArm)
                {
                    selected++;
                }

                if (this.FilterArm64)
                {
                    selected++;
                }

                if (this.FilterNeutral)
                {
                    selected++;
                }

                return $"({selected}/5)";
            }
        }

        public bool FilterHasAppInstaller
        {
            get => (this._application.ApplicationState.Packages.Filter & PackageFilter.HasAppInstaller) == PackageFilter.HasAppInstaller;
            set
            {
                var newValue = this._application.ApplicationState.Packages.Filter & ~(PackageFilter.HasAppInstaller | PackageFilter.NoAppInstaller);
                if (value)
                {
                    newValue |= PackageFilter.HasAppInstaller;
                }

                this.SetPackageFilter(newValue);
            }
        }

        public bool FilterRunning
        {
            get => (this._application.ApplicationState.Packages.Filter & PackageFilter.InstalledAndRunning) == PackageFilter.Running;
            set
            {
                var newValue = this._application.ApplicationState.Packages.Filter & ~PackageFilter.InstalledAndRunning;
                if (value)
                {
                    newValue |= PackageFilter.Running;
                }
                else
                {
                    newValue |= PackageFilter.InstalledAndRunning;
                }

                this.SetPackageFilter(newValue);
            }
        }

        private void OnSetPackageSorting(UiExecutedPayload<SetPackageSortingCommand> obj)
        {
            this.OnPropertyChanged(nameof(IsDescending));
            this.OnPropertyChanged(nameof(Sort));
        }

        private void OnSetPackageGrouping(UiExecutedPayload<SetPackageGroupingCommand> obj)
        {
            this.OnPropertyChanged(nameof(Group));
        }

        private void BusyManagerOnStatusChanged(object sender, IBusyStatusChange e)
        {
            if (e.Type != OperationType.PackageLoading)
            {
                return;
            }

            this.IsBusy = e.IsBusy;
            this.Progress = e.Progress;
        }

        private void OnSetPackageFilter(UiExecutedPayload<SetPackageFilterCommand> obj)
        {
            this.OnPropertyChanged(nameof(FilterSystem));
            this.OnPropertyChanged(nameof(FilterSideLoaded));
            this.OnPropertyChanged(nameof(FilterStore));

            this.OnPropertyChanged(nameof(FilterRunning));

            this.OnPropertyChanged(nameof(FilterHasAppInstaller));

            this.OnPropertyChanged(nameof(FilterX64));
            this.OnPropertyChanged(nameof(FilterX86));
            this.OnPropertyChanged(nameof(FilterArm));
            this.OnPropertyChanged(nameof(FilterArm64));
            this.OnPropertyChanged(nameof(FilterNeutral));

            this.OnPropertyChanged(nameof(FilterMainApp));
            this.OnPropertyChanged(nameof(FilterAddOn));

            this.OnPropertyChanged(nameof(FilterCategoryCaption));
            this.OnPropertyChanged(nameof(FilterArchitectureCaption));
            this.OnPropertyChanged(nameof(FilterTypeCaption));
        }

        private void OnGetPackages(UiFailedPayload<GetInstalledPackagesCommand> _)
        {
            this._source = this._application.ApplicationState.Packages.Mode;
            this.OnPropertyChanged(nameof(Source));
        }

        private void OnGetPackages(UiExecutedPayload<GetInstalledPackagesCommand> _)
        {
            this._source = this._application.ApplicationState.Packages.Mode;
            this.OnPropertyChanged(nameof(Source));
        }

        private void OnGetPackages(UiCancelledPayload<GetInstalledPackagesCommand> _)
        {
            this._source = this._application.ApplicationState.Packages.Mode;
            this.OnPropertyChanged(nameof(Source));
        }

        private async void LoadContext(PackageInstallationContext mode)
        {
            var executor = this._application.CommandExecutor
                .WithBusyManager(this._busyManager, OperationType.PackageLoading)
                .WithErrorHandling(this._interactionService, true);

            switch (mode)
            {
                case PackageInstallationContext.AllUsers:
                    await executor.Invoke<GetInstalledPackagesCommand, IList<PackageEntry>>(this, new GetInstalledPackagesCommand(PackageFindMode.AllUsers), CancellationToken.None).ConfigureAwait(false);
                    break;
                case PackageInstallationContext.CurrentUser:
                    await executor.Invoke<GetInstalledPackagesCommand, IList<PackageEntry>>(this, new GetInstalledPackagesCommand(PackageFindMode.CurrentUser), CancellationToken.None).ConfigureAwait(false);
                    break;
            }
        }

        private void SetPackageFilter(PackageFilter packageFilter, bool isSet)
        {
            var currentFilter = this._application.ApplicationState.Packages.Filter;
            if (isSet)
            {
                currentFilter |= packageFilter;
            }
            else
            {
                currentFilter &= ~packageFilter;
            }

            this.SetPackageFilter(currentFilter);
        }

        private void SetPackageFilter(PackageFilter packageFilter)
        {
            var state = this._application.ApplicationState.Packages;
            this._application.CommandExecutor.WithErrorHandling(this._interactionService, false).Invoke(this, new SetPackageFilterCommand(packageFilter, state.SearchKey));
        }
        
        private void OnClearFilter(object objectFilterToClear)
        {
            if (!(objectFilterToClear is ClearFilter filterToClear))
            {
                return;
            }

            switch (filterToClear)
            {
                case ClearFilter.Architecture:
                    this.SetPackageFilter(PackageFilter.AllArchitectures, true);
                    break;
                case ClearFilter.Activity:
                    this.SetPackageFilter(PackageFilter.InstalledAndRunning, true);
                    break;
                case ClearFilter.AppInstaller:
                    this.SetPackageFilter(PackageFilter.HasAppInstaller, false);
                    this.SetPackageFilter(PackageFilter.NoAppInstaller, false);
                    break;
                case ClearFilter.Category:
                    this.SetPackageFilter(PackageFilter.AllSources, true);
                    break;
                case ClearFilter.Type:
                    this.SetPackageFilter(PackageFilter.MainAppsAndAddOns, true);
                    break;
            }
        }
    }
}
