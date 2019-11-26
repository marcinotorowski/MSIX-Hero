using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows.Shapes;
using otor.msixhero.lib;
using otor.msixhero.lib.BusinessLayer.Commands;
using otor.msixhero.lib.BusinessLayer.Commands.Grid;
using otor.msixhero.lib.BusinessLayer.Commands.UI;
using otor.msixhero.lib.BusinessLayer.Events;
using otor.msixhero.lib.BusinessLayer.Infrastructure;
using otor.msixhero.lib.BusinessLayer.Models.Configuration;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.BusinessLayer.State.Enums;
using otor.msixhero.lib.Managers;
using otor.msixhero.lib.Services;
using otor.msixhero.ui.Services;
using otor.msixhero.ui.ViewModel;
using Prism.Events;
using Prism.Regions;
using Prism.Services.Dialogs;
using Path = System.IO.Path;

namespace otor.msixhero.ui.Modules.Main.ViewModel
{
    public class MainViewModel : NotifyPropertyChanged
    {
        private readonly IInteractionService interactionService;
        private readonly IApplicationStateManager stateManager;
        private readonly IAppxPackageManagerFactory appxPackageManagerFactory;
        private readonly IRegionManager regionManager;
        private readonly IDialogService dialogService;
        private bool isLoading;
        private string loadingMessage;
        private int loadingProgress;

        public MainViewModel(
            IInteractionService interactionService,
            IApplicationStateManager stateManager,
            IAppxPackageManagerFactory appxPackageManagerFactory, 
            IRegionManager regionManager, 
            IDialogService dialogService, 
            IBusyManager busyManager)
        {
            this.interactionService = interactionService;
            this.stateManager = stateManager;
            this.appxPackageManagerFactory = appxPackageManagerFactory;
            this.regionManager = regionManager;
            this.dialogService = dialogService;
            this.Tools = new ObservableCollection<ToolViewModel>();

            busyManager.StatusChanged += this.BusyManagerOnStatusChanged;
            stateManager.EventAggregator.GetEvent<PackagesCollectionChanged>().Subscribe(this.OnPackageLoaded, ThreadOption.UIThread);
            stateManager.EventAggregator.GetEvent<PackagesFilterChanged>().Subscribe(this.OnPackageFilterChanged);
            stateManager.EventAggregator.GetEvent<PackagesSelectionChanged>().Subscribe(this.OnPackagesSelectionChanged, ThreadOption.UIThread);
            stateManager.EventAggregator.GetEvent<PackageGroupAndSortChanged>().Subscribe(this.OnPackageGroupAndSortChanged, ThreadOption.UIThread);
            stateManager.EventAggregator.GetEvent<PackagesSidebarVisibilityChanged>().Subscribe(this.OnPackagesSidebarVisibilityChanged);

            this.SetTools();
        }

        private void SetTools()
        {
            this.Tools.Clear();

            foreach (var item in this.stateManager.CurrentState.Configuration?.List?.Tools ?? Enumerable.Empty<ToolListConfiguration>())
            {
                var itemName = item.Name;
                if (string.IsNullOrEmpty(itemName) && !string.IsNullOrEmpty(item.Path))
                {
                    itemName = Path.GetFileNameWithoutExtension(item.Path);
                }

                this.Tools.Add(new ToolViewModel(itemName, item.Path));
            }
        }

        public CommandHandler CommandHandler => new CommandHandler(this.interactionService, this.stateManager, this.dialogService);

        public ObservableCollection<ToolViewModel> Tools { get; }

        public PackageContext Context
        {
            get => this.stateManager.CurrentState.Packages.Context;
            set => this.stateManager.CommandExecutor.ExecuteAsync(new SetPackageContext(value));
        }

        public bool HasSelection => this.stateManager.CurrentState.Packages.SelectedItems.Any();

        public bool HasSingleSelection => this.stateManager.CurrentState.Packages.SelectedItems.Count == 1;

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

        public PackageGroup Group
        {
            get => this.stateManager.CurrentState.Packages.Group;
            set => this.stateManager.CommandExecutor.ExecuteAsync(new SetPackageGrouping(value), CancellationToken.None);
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
            this.OnPropertyChanged(nameof(HasSingleSelection));
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

        private void OnPackagesSidebarVisibilityChanged(bool newVisibility)
        {
            this.OnPropertyChanged(nameof(ShowSidebar));
        }

        private void OnPackageGroupAndSortChanged(PackageGroupAndSortChangedPayload obj)
        {
            this.OnPropertyChanged(nameof(Group));
        }

        private void OnPackageLoaded(PackagesCollectionChangedPayLoad obj)
        {
            this.OnPropertyChanged(nameof(Context));
        }
    }
}
