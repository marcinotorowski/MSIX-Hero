using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Appx.Packages;
using otor.msixhero.lib.Domain.Commands.Packages.Grid;
using otor.msixhero.lib.Domain.Events;
using otor.msixhero.lib.Domain.Events.PackageList;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Configuration;
using otor.msixhero.lib.Infrastructure.Progress;
using otor.msixhero.ui.Modules.Dialogs.PackageExpert.ViewModel;
using otor.msixhero.ui.ViewModel;
using Prism.Events;
using Prism.Services.Dialogs;
using Path = System.IO.Path;

namespace otor.msixhero.ui.Modules.Main.ViewModel
{
    public class MainViewModel : NotifyPropertyChanged
    {
        private readonly IInteractionService interactionService;
        private readonly IApplicationStateManager stateManager;
        private readonly IConfigurationService configurationService;
        private readonly IDialogService dialogService;
        private readonly IBusyManager busyManager;
        private bool isLoading;
        private string loadingMessage;
        private int loadingProgress;
        private PackageContext? tempContext;

        public MainViewModel(
            IInteractionService interactionService,
            IApplicationStateManager stateManager, 
            IConfigurationService configurationService, 
            IDialogService dialogService,
            IBusyManager busyManager)
        {
            this.interactionService = interactionService;
            this.stateManager = stateManager;
            this.configurationService = configurationService;
            this.dialogService = dialogService;
            this.busyManager = busyManager;
            this.Tools = new ObservableCollection<ToolViewModel>();

            busyManager.StatusChanged += this.BusyManagerOnStatusChanged;
            stateManager.EventAggregator.GetEvent<PackagesCollectionChanged>().Subscribe(this.OnPackageLoaded);
            stateManager.EventAggregator.GetEvent<PackagesFilterChanged>().Subscribe(this.OnPackageFilterChanged);
            stateManager.EventAggregator.GetEvent<PackagesSelectionChanged>().Subscribe(this.OnPackagesSelectionChanged);
            stateManager.EventAggregator.GetEvent<PackageGroupAndSortChanged>().Subscribe(this.OnPackageGroupAndSortChanged);
            stateManager.EventAggregator.GetEvent<PackagesSidebarVisibilityChanged>().Subscribe(this.OnPackagesSidebarVisibilityChanged);
            stateManager.EventAggregator.GetEvent<ToolsChangedEvent>().Subscribe(this.OnToolsChangedEvent, ThreadOption.UIThread);
            this.SetTools();
        }

        private void OnToolsChangedEvent(IReadOnlyCollection<ToolListConfiguration> toolsCollection)
        {
            this.SetTools(toolsCollection);
        }

        private void SetTools(IEnumerable<ToolListConfiguration> tools = null)
        {
            this.Tools.Clear();

            if (tools == null)
            {
                var config = this.configurationService.GetCurrentConfiguration();
                tools = config?.List?.Tools ?? Enumerable.Empty<ToolListConfiguration>();
            }

            foreach (var item in tools)
            {
                var itemName = item.Name;
                if (string.IsNullOrEmpty(itemName) && !string.IsNullOrEmpty(item.Path))
                {
                    itemName = Path.GetFileNameWithoutExtension(item.Path);
                }

                this.Tools.Add(new ToolViewModel(itemName, item));
            }
        }

        public MainCommandHandler CommandHandler => new MainCommandHandler(this.dialogService);
        public BackstageCommandHandler BackstageCommandHandler => new BackstageCommandHandler(this.dialogService, this.interactionService);

        public ObservableCollection<ToolViewModel> Tools { get; }
        
        public PackageContext Context
        {
            get
            {
                if (this.tempContext.HasValue)
                {
                    return this.tempContext.Value;
                }

                return this.stateManager.CurrentState.Packages.Context;
            }
            set
            {
                if (this.Context == value)
                {
                    return;
                }

                this.tempContext = value;
                this.OnPropertyChanged();

                var context = this.busyManager.Begin();

                this.stateManager.CommandExecutor.GetExecuteAsync(new GetPackages(value), CancellationToken.None, context).ContinueWith(
                    t =>
                    {
                        this.busyManager.End(context);
                        this.tempContext = null;
                        this.OnPropertyChanged(nameof(this.Context));
                    },
                CancellationToken.None,
                TaskContinuationOptions.AttachedToParent,
                TaskScheduler.FromCurrentSynchronizationContext());
            }
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
            get => this.stateManager.CurrentState.Packages.ShowSidebar;
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

                this.stateManager.CommandExecutor.ExecuteAsync(SetPackageFilter.CreateFrom(currentFilter));
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
