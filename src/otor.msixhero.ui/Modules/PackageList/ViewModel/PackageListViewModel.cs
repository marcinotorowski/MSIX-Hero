using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Appx.Packages;
using otor.msixhero.lib.Domain.Commands.Generic;
using otor.msixhero.lib.Domain.Commands.Packages.Grid;
using otor.msixhero.lib.Domain.Events.PackageList;
using otor.msixhero.lib.Domain.State;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Configuration;
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
        private bool isActive;
        private bool firstRun = true;

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
                    this.stateManager.CommandExecutor.ExecuteAsync(new SetMode(ApplicationMode.Packages));

                    if (this.firstRun)
                    {
                        firstRun = false;

                        var context = this.busyManager.Begin();
                        this.stateManager.CommandExecutor.GetExecuteAsync(new GetPackages(this.stateManager.CurrentState.Packages.Context)).ContinueWith(
                            t =>
                            {
                                this.busyManager.End(context);
                            }, 
                            TaskContinuationOptions.AttachedToParent);
                    }
                }
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
                    item.IsSelected = true;
                }
                else if (selectionInfo.Unselected.Contains(item.Model))
                {
                    item.IsSelected = false;
                }

                if (item.IsSelected && countSelected < 2)
                {
                    countSelected++;
                }
            }


            var selected = this.AllPackages.Where(p => p.IsSelected).Select(p => p.ManifestLocation).ToArray();
            var navigation = new PackageListNavigation(selected);
            switch (selected.Length)
            {
                case 0:
                {
                    this.regionManager.Regions["PackageSidebar"].RequestNavigate(new Uri(PackageListModule.SidebarEmptySelection, UriKind.Relative), navigation.ToParameters());
                    break;
                }

                case 1:
                {
                    this.regionManager.Regions["PackageSidebar"].RequestNavigate(new Uri(PackageListModule.SidebarSingleSelection, UriKind.Relative), navigation.ToParameters());
                    break;
                }

                default:
                {
                    this.regionManager.Regions["PackageSidebar"].RequestNavigate(new Uri(PackageListModule.SidebarMultiSelection, UriKind.Relative), navigation.ToParameters());
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

        private void OnPackagesLoaded(PackagesCollectionChangedPayLoad payload)
        {
            var selectedItems = this.AllPackages.Where(a => a.IsSelected).Select(a => a.Model.ManifestLocation).ToList();

            switch (payload.Type)
            {
                case CollectionChangeType.Reset:
                    this.AllPackages.Clear();
                    var visibleItems = this.stateManager.CurrentState.Packages.VisibleItems.ToArray();
                    foreach (var item in visibleItems)
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
            this.stateManager.CommandExecutor.ExecuteAsync(new SetMode(ApplicationMode.Packages));
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
            set => this.stateManager.CommandExecutor.ExecuteAsync(SetPackageFilter.CreateFrom(value));
        }

        public ObservableCollection<InstalledPackageViewModel> AllPackages { get; }

        public ICollectionView AllPackagesView { get; }
    }
}
