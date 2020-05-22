using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Appx.Packages;
using otor.msixhero.lib.Domain.Commands.Generic;
using otor.msixhero.lib.Domain.Commands.Packages.Grid;
using otor.msixhero.lib.Domain.Events.PackageList;
using otor.msixhero.lib.Domain.State;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Commanding;
using otor.msixhero.lib.Infrastructure.Configuration;
using otor.msixhero.lib.Infrastructure.Progress;
using otor.msixhero.ui.Commands.RoutedCommand;
using otor.msixhero.ui.Modules.Dialogs.PackageExpert.ViewModel;
using otor.msixhero.ui.Modules.PackageList.Navigation;
using otor.msixhero.ui.Modules.PackageList.ViewModel.Elements;
using otor.msixhero.ui.Themes;
using otor.msixhero.ui.ViewModel;
using Prism;
using Prism.Events;
using Prism.Regions;
using Prism.Services.Dialogs;

namespace otor.msixhero.ui.Modules.PackageList.ViewModel
{
    public interface IHeaderViewModel
    {
        string Header { get; }

        Geometry Icon { get; }
    }

    public class PackageListViewModel : NotifyPropertyChanged, INavigationAware, IActiveAware, IHeaderViewModel
    {
        private readonly IApplicationStateManager stateManager;
        private readonly IInteractionService interactionService;
        private readonly IConfigurationService configurationService;
        private readonly IDialogService dialogService;
        private readonly IRegionManager regionManager;
        private readonly IBusyManager busyManager;
        private readonly List<string> previousSelection = new List<string>();
        private bool isActive;
        private bool firstRun = true;
        private ICommand showSelectionDetails;
        private bool isLoading;
        private int loadingProgress;
        private string loadingMessage;
        private PackageContext? tempContext;

        public PackageListViewModel(
            IApplicationStateManager stateManager,
            IInteractionService interactionService,
            IConfigurationService configurationService,
            IDialogService dialogService,
            IRegionManager regionManager,
            IBusyManager busyManager)
        {
            this.stateManager = stateManager;
            this.interactionService = interactionService;
            this.configurationService = configurationService;
            this.dialogService = dialogService;
            this.regionManager = regionManager;
            this.busyManager = busyManager;

            this.AllPackages = new ObservableCollection<InstalledPackageViewModel>();
            this.AllPackagesView = CollectionViewSource.GetDefaultView(this.AllPackages);
            this.SetSortingAndGrouping();

            stateManager.EventAggregator.GetEvent<PackagesFilterChanged>().Subscribe(this.OnPackageFilterChanged, ThreadOption.UIThread);
            stateManager.EventAggregator.GetEvent<PackagesCollectionChanged>().Subscribe(this.OnPackagesLoaded, ThreadOption.UIThread);
            stateManager.EventAggregator.GetEvent<PackagesVisibilityChanged>().Subscribe(this.OnPackagesVisibilityChanged, ThreadOption.UIThread);
            stateManager.EventAggregator.GetEvent<PackagesSelectionChanged>().Subscribe(this.OnPackagesSelectionChanged, ThreadOption.UIThread);
            stateManager.EventAggregator.GetEvent<PackageGroupAndSortChanged>().Subscribe(this.OnGroupAndSortChanged, ThreadOption.UIThread);

            this.busyManager.StatusChanged += this.BusyManagerOnStatusChanged;
        }

        private void BusyManagerOnStatusChanged(object sender, IBusyStatusChange e)
        {
            if (e.Type != OperationType.PackageLoading)
            {
                return;
            }

            this.IsLoading = e.IsBusy;
            this.LoadingMessage = e.Message;
            this.LoadingProgress = e.Progress;
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

        public PackageListCommandHandler CommandHandler => new PackageListCommandHandler(this.interactionService, this.configurationService, this.stateManager, this.dialogService, this.busyManager);

        public bool IsActive
        {
            get => this.isActive;
            set
            {
                if (this.isActive == value)
                {
                    return;
                }

                this.isActive = value;
                this.IsActiveChanged?.Invoke(this, new EventArgs());
                
                if (value)
                {
#pragma warning disable 4014
                    this.SetTabActive();
#pragma warning restore 4014
                }
            }
        }

        private async Task SetTabActive()
        {
            try
            {
                await this.stateManager.CommandExecutor.ExecuteAsync(new SetMode(ApplicationMode.Packages)).ConfigureAwait(false);
            }
            catch (UserHandledException)
            {
            }

            if (!this.firstRun)
            {
                return;
            }

            firstRun = false;

            var context = this.busyManager.Begin(OperationType.PackageLoading);

            try
            {
                context.Message = "Reading packages...";
                await Task.Delay(200).ConfigureAwait(false);
                await this.stateManager.CommandExecutor.GetExecuteAsync(new GetPackages(this.stateManager.CurrentState.Packages.Context)).ConfigureAwait(false);
            }
            catch (UserHandledException)
            {
            }
            finally
            {
                this.busyManager.End(context);
            }
        }

        public event EventHandler IsActiveChanged;

        public string Header { get; } = "Packages";

        public Geometry Icon { get; } = VectorIcons.TabPackages;
        
        private void OnPackagesSelectionChanged(PackagesSelectionChangedPayLoad selectionInfo)
        {
            var countSelected = 0;

            foreach (var item in this.AllPackages)
            {
                if (selectionInfo.Selected.Contains(item.Model))
                {
                    if (!item.IsSelected)
                    {
                        item.IsSelected = true;
                    }
                }
                else if (selectionInfo.Unselected.Contains(item.Model))
                {
                    if (item.IsSelected)
                    {
                        item.IsSelected = false;
                    }
                }

                if (item.IsSelected && countSelected < 2)
                {
                    countSelected++;
                }
            }

            var selected = this.AllPackages.Where(p => p.IsSelected).Select(p => p.ManifestLocation).ToArray();

            if (selected.SequenceEqual(this.previousSelection))
            {
                return;
            }

            this.previousSelection.Clear();
            this.previousSelection.AddRange(selected);

            var navigation = new PackageListNavigation(selected);
            switch (selected.Length)
            {
                case 0:
                {
                    this.regionManager.Regions[Constants.RegionPackageSidebar].RequestNavigate(new Uri(Constants.PathPackagesEmptySelection, UriKind.Relative), navigation.ToParameters());
                    break;
                }

                case 1:
                {
                    this.regionManager.Regions[Constants.RegionPackageSidebar].RequestNavigate(new Uri(Constants.PathPackagesSingleSelection, UriKind.Relative), navigation.ToParameters());
                    break;
                }

                default:
                {
                    this.regionManager.Regions[Constants.RegionPackageSidebar].RequestNavigate(new Uri(Constants.PathPackagesMultiSelection, UriKind.Relative), navigation.ToParameters());
                    break;
                }
            }
        }

        private void OnGroupAndSortChanged(PackageGroupAndSortChangedPayload payload)
        {
            this.SetSortingAndGrouping();
        }

        private void SetSortingAndGrouping()
        {
            var currentSort = this.stateManager.CurrentState.Packages.Sort;
            var currentSortDescending = this.stateManager.CurrentState.Packages.SortDescending;
            var currentGroup = this.stateManager.CurrentState.Packages.Group;

            using (this.AllPackagesView.DeferRefresh())
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
                    case PackageGroup.Type:
                        groupProperty = nameof(InstalledPackageViewModel.Type);
                        break;
                    default:
                        return;
                }

                // 1) First grouping
                if (groupProperty == null)
                {
                    this.AllPackagesView.GroupDescriptions.Clear();
                }
                else
                {
                    var pgd = this.AllPackagesView.GroupDescriptions.OfType<PropertyGroupDescription>().FirstOrDefault();
                    if (pgd == null || pgd.PropertyName != groupProperty)
                    {
                        this.AllPackagesView.GroupDescriptions.Clear();
                        this.AllPackagesView.GroupDescriptions.Add(new PropertyGroupDescription(groupProperty));
                    }
                }

                // 2) Then sorting
                if (sortProperty == null)
                {
                    this.AllPackagesView.SortDescriptions.Clear();
                }
                else
                {
                    var sd = this.AllPackagesView.SortDescriptions.FirstOrDefault();
                    if (sd.PropertyName != sortProperty || sd.Direction != (currentSortDescending ? ListSortDirection.Descending : ListSortDirection.Ascending))
                    {
                        this.AllPackagesView.SortDescriptions.Clear();
                        this.AllPackagesView.SortDescriptions.Add(new SortDescription(sortProperty, currentSortDescending ? ListSortDirection.Descending : ListSortDirection.Ascending));
                    }
                }

                if (this.AllPackagesView.GroupDescriptions.Any())
                {
                    var gpn = ((PropertyGroupDescription)this.AllPackagesView.GroupDescriptions[0]).PropertyName;
                    if (this.AllPackagesView.GroupDescriptions.Any() && this.AllPackagesView.SortDescriptions.All(sd => sd.PropertyName != gpn))
                    {
                        this.AllPackagesView.SortDescriptions.Insert(0, new SortDescription(gpn, ListSortDirection.Ascending));
                    }
                }
            }
        }

        private void OnPackagesVisibilityChanged(PackagesVisibilityChangedPayLoad visibilityInfo)
        {
            for (var i = this.AllPackages.Count - 1; i >= 0 ; i--)
            {
                var item = this.AllPackages[i];
                if (visibilityInfo.NewHidden.Contains(item.Model))
                {
                    this.AllPackages.RemoveAt(i);
                }
            }

            foreach (var item in visibilityInfo.NewVisible)
            {
                this.AllPackages.Add(new InstalledPackageViewModel(item, this.stateManager));
            }
        }

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

                var context = this.busyManager.Begin(OperationType.PackageLoading);

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

        private void OnPackagesLoaded(PackagesCollectionChangedPayLoad payload)
        {
            this.OnPropertyChanged(nameof(Context));

            var selectedItems = this.AllPackages.Where(a => a.IsSelected).Select(a => a.Model.ManifestLocation).ToList();

            switch (payload.Type)
            {
                case CollectionChangeType.Reset:
                    this.AllPackages.Clear();
                    var visibleItems = this.stateManager.CurrentState.Packages.VisibleItems.ToArray();
                    foreach (var item in visibleItems.Where(visibleItem => visibleItem != null))
                    {
                        var isSelected = item.ManifestLocation != null && selectedItems.Contains(item.ManifestLocation);
                        this.AllPackages.Add(new InstalledPackageViewModel(item, this.stateManager, isSelected));
                    }

                    break;

                case CollectionChangeType.Simple:

                    for (var i = this.AllPackages.Count - 1; i >= 0; i--)
                    {
                        if (payload.OldPackages.Contains(this.AllPackages[i].Model))
                        {
                            this.AllPackages.RemoveAt(i);
                        }
                    }

                    foreach (var item in payload.NewPackages)
                    {
                        var isSelected = selectedItems.Contains(item.PackageId);
                        this.AllPackages.Add(new InstalledPackageViewModel(item, this.stateManager, isSelected));
                    }

                    break;
            }
        }

        private void OnPackageFilterChanged(PackagesFilterChangedPayload filter)
        {
            if (filter.NewSearchKey != filter.OldSearchKey)
            {
                this.OnPropertyChanged(nameof(SearchKey));
            }
        }
        
        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            try
            {
                this.stateManager.CommandExecutor.ExecuteAsync(new SetMode(ApplicationMode.Packages));
            }
            catch (UserHandledException)
            {
            }
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        public string SearchKey
        {
            get => this.stateManager.CurrentState.Packages.SearchKey;
            set => this.stateManager.CommandExecutor.ExecuteAsync(SetPackageFilter.CreateFrom(this.stateManager.CurrentState.Packages.Filter, value));
        }

        public ObservableCollection<InstalledPackageViewModel> AllPackages { get; }

        public ICollectionView AllPackagesView { get; }

        public ICommand ShowSelectionDetails
        {
            get
            {
                return this.showSelectionDetails ??= new DelegateCommand(this.ShowSelectionDetailsExecute);
            }
        }

        private void ShowSelectionDetailsExecute(object obj)
        {
            var selected = this.AllPackages.FirstOrDefault(item => item.IsSelected);
            if (selected == null)
            {
                return;
            }

            this.dialogService.ShowDialog(Constants.PathPackageExpert, new PackageExpertSelection(selected.ManifestLocation).ToDialogParameters(), result => {});
        }
    }
}
