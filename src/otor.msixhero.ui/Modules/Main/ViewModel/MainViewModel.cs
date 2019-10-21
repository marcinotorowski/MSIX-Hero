﻿using System.Collections.ObjectModel;
using System.Linq;
using MSI_Hero.Domain;
using MSI_Hero.Domain.Actions;
using MSI_Hero.Domain.Events;
using MSI_Hero.Domain.State.Enums;
using MSI_Hero.Services;
using otor.msihero.lib;
using Prism.Events;
using Prism.Regions;
using Prism.Services.Dialogs;

namespace MSI_Hero.ViewModel
{
    public class MainViewModel : NotifyPropertyChanged
    {
        private readonly IApplicationStateManager stateManager;
        private readonly IAppxPackageManager appxPackageManager;
        private readonly IRegionManager regionManager;
        private readonly IDialogService dialogService;
        private bool isLoading;
        private string loadingMessage;
        private int loadingProgress;

        public MainViewModel(
            IApplicationStateManager stateManager,
            IAppxPackageManager appxPackageManager, 
            IRegionManager regionManager, 
            IDialogService dialogService, 
            IBusyManager busyManager)
        {
            this.stateManager = stateManager;
            this.appxPackageManager = appxPackageManager;
            this.regionManager = regionManager;
            this.dialogService = dialogService;
            this.Tools = new ObservableCollection<ToolViewModel>();
            this.Tools.Add(new ToolViewModel("notepad.exe"));
            this.Tools.Add(new ToolViewModel("regedit.exe"));
            this.Tools.Add(new ToolViewModel("powershell.exe"));
            this.Tools.Add(new ToolViewModel("cmd.exe"));

            busyManager.StatusChanged += BusyManagerOnStatusChanged;

            stateManager.EventAggregator.GetEvent<PackagesLoadedEvent>().Subscribe(this.OnPackageLoaded, ThreadOption.UIThread);
            stateManager.EventAggregator.GetEvent<PackagesFilterChanged>().Subscribe(this.OnPackageFilterChanged, ThreadOption.UIThread);
            stateManager.EventAggregator.GetEvent<PackagesSelectionChanged>().Subscribe(this.OnPackagesSelectionChanged, ThreadOption.UIThread);
        }

        public CommandHandler CommandHandler => new CommandHandler(this.stateManager, this.appxPackageManager, this.regionManager, this.dialogService);

        public ObservableCollection<ToolViewModel> Tools { get; }

        public PackageContext Context
        {
            get => this.stateManager.CurrentState.Packages.Context;
            set => this.stateManager.Executor.ExecuteAsync(new SetPackageContext(value));
        }

        public bool HasSelection
        {
            get
            {
                return this.stateManager.CurrentState.Packages.SelectedItems.Any();
            }
        }
        
        public bool ShowSideLoadedApps
        {
            get => (this.stateManager.CurrentState.Packages.Filter & PackageFilter.Developer) == PackageFilter.Developer;
            set
            {
                var currentFilter = this.stateManager.CurrentState.Packages.Filter;
                if (value)
                {
                    currentFilter |= PackageFilter.Developer;
                }
                else
                {
                    currentFilter &= ~PackageFilter.Developer;
                }

                this.stateManager.Executor.ExecuteAsync(SetPackageFilter.CreateFrom(currentFilter));
            }
        }

        public bool ShowStoreApps
        {
            get => (this.stateManager.CurrentState.Packages.Filter & PackageFilter.Store) == PackageFilter.Store;
            set
            {
                var currentFilter = this.stateManager.CurrentState.Packages.Filter;
                if (value)
                {
                    currentFilter |= PackageFilter.Store;
                }
                else
                {
                    currentFilter &= ~PackageFilter.Store;
                }

                this.stateManager.Executor.ExecuteAsync(SetPackageFilter.CreateFrom(currentFilter));
            }
        }

        public bool ShowSystemApps
        {
            get => (this.stateManager.CurrentState.Packages.Filter & PackageFilter.System) == PackageFilter.System;
            set
            {
                var currentFilter = this.stateManager.CurrentState.Packages.Filter;
                if (value)
                {
                    currentFilter |= PackageFilter.System;
                }
                else
                {
                    currentFilter &= ~PackageFilter.System;
                }

                this.stateManager.Executor.Execute(SetPackageFilter.CreateFrom(currentFilter));
            }
        }

        public bool IsLoading
        {
            get => this.isLoading;
            private set => this.SetField(ref this.isLoading, value);
        }

        public int LoadingProgress
        {
            get => this.loadingProgress;
            private set => this.SetField(ref this.loadingProgress, value);
        }

        public string LoadingMessage
        {
            get => this.loadingMessage;
            private set => this.SetField(ref this.loadingMessage, value);
        }

        private void BusyManagerOnStatusChanged(object sender, IBusyStatusChange e)
        {
            this.IsLoading = e.IsBusy;
            this.LoadingMessage = e.Message;
            this.LoadingProgress = e.Progress;
        }
        private void OnPackagesSelectionChanged(PackagesSelectionChangedPayLoad selectionInfo)
        {
            this.OnPropertyChanged(nameof(HasSelection));
        }

        private void OnPackageFilterChanged(PackagesFilterChangedPayload obj)
        {
            if (obj.NewFilter != obj.OldFilter)
            {
                this.OnPropertyChanged(nameof(ShowSystemApps));
                this.OnPropertyChanged(nameof(ShowSideLoadedApps));
                this.OnPropertyChanged(nameof(ShowStoreApps));
            }
        }

        private void OnPackageLoaded(PackageContext obj)
        {
            this.OnPropertyChanged(nameof(Context));
        }
    }
}
