using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using otor.msixhero.lib;
using otor.msixhero.lib.BusinessLayer.Commands;
using otor.msixhero.lib.BusinessLayer.Events;
using otor.msixhero.lib.BusinessLayer.Infrastructure;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.BusinessLayer.State.Enums;
using otor.msixhero.ui.ViewModel;
using Prism.Events;
using Prism.Regions;
using Prism.Services.Dialogs;

namespace otor.msixhero.ui.Modules.Main.ViewModel
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

            busyManager.StatusChanged += this.BusyManagerOnStatusChanged;

            stateManager.EventAggregator.GetEvent<PackagesLoadedEvent>().Subscribe(this.OnPackageLoaded, ThreadOption.UIThread);
            stateManager.EventAggregator.GetEvent<PackagesFilterChanged>().Subscribe(this.OnPackageFilterChanged, ThreadOption.UIThread);
            stateManager.EventAggregator.GetEvent<PackagesSelectionChanged>().Subscribe(this.OnPackagesSelectionChanged, ThreadOption.UIThread);
            stateManager.EventAggregator.GetEvent<PackagesSidebarVisibilityChanged>().Subscribe(this.OnPackagesSidebarVisibilityChanged);
        }

        private void OnPackagesSidebarVisibilityChanged(bool newVisibility)
        {
            this.OnPropertyChanged(nameof(ShowSidebar));
        }

        public CommandHandler CommandHandler => new CommandHandler(this.stateManager, this.appxPackageManager, this.regionManager, this.dialogService);

        public ObservableCollection<ToolViewModel> Tools { get; }

        public PackageContext Context
        {
            get => this.stateManager.CurrentState.Packages.Context;
            set => this.stateManager.CommandExecutor.ExecuteAsync(new SetPackageContext(value));
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

                this.stateManager.CommandExecutor.ExecuteAsync(SetPackageFilter.CreateFrom(currentFilter));
            }
        }

        public bool ShowSidebar
        {
            get => this.stateManager.CurrentState.LocalSettings.ShowSidebar;
            set => this.stateManager.CommandExecutor.ExecuteAsync(new SetPackageSidebarVisibility(value), CancellationToken.None);
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

                this.stateManager.CommandExecutor.ExecuteAsync(SetPackageFilter.CreateFrom(currentFilter));
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

                this.stateManager.CommandExecutor.Execute(SetPackageFilter.CreateFrom(currentFilter));
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
