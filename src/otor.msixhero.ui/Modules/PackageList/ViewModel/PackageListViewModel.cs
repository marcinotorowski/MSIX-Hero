using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Appx.Packages;
using otor.msixhero.lib.Domain.Commands.Grid;
using otor.msixhero.lib.Domain.Events;
using otor.msixhero.ui.ViewModel;
using Prism;
using Prism.Events;
using Prism.Regions;

namespace otor.msixhero.ui.Modules.PackageList.ViewModel
{
    public class PackageListViewModel : NotifyPropertyChanged, INavigationAware, IActiveAware
    {
        private readonly IApplicationStateManager stateManager;
        private readonly IRegionManager regionManager;

        public PackageListViewModel(IApplicationStateManager stateManager, IRegionManager regionManager)
        {
            this.stateManager = stateManager;
            this.regionManager = regionManager;

            this.AllPackages = new ObservableCollection<PackageViewModel>();
            this.AllPackagesView = CollectionViewSource.GetDefaultView(this.AllPackages);
            this.SetSortingAndGrouping();

            stateManager.EventAggregator.GetEvent<PackagesFilterChanged>().Subscribe(this.OnPackageFilterChanged);
            stateManager.EventAggregator.GetEvent<PackagesCollectionChanged>().Subscribe(this.OnPackagesLoaded, ThreadOption.UIThread);
            stateManager.EventAggregator.GetEvent<PackagesVisibilityChanged>().Subscribe(this.OnPackagesVisibilityChanged);
            stateManager.EventAggregator.GetEvent<PackagesSelectionChanged>().Subscribe(this.OnPackagesSelectionChanged);
            stateManager.EventAggregator.GetEvent<PackageGroupAndSortChanged>().Subscribe(this.OnGroupAndSortChanged);

            this.RefreshPackages();
        }

        private void OnPackagesSelectionChanged(PackagesSelectionChangedPayLoad selectionInfo)
        {
            for (var i = this.SelectedPackages.Count - 1; i >= 0; i--)
            {
                if (!selectionInfo.Selected.Contains(this.SelectedPackages[i].Model))
                {
                    this.SelectedPackages.RemoveAt(i);
                }
            }

            foreach (var item in selectionInfo.Selected)
            {
                var vm = this.SelectedPackages.FirstOrDefault(m => m.Model == item);
                if (vm != null)
                {
                    continue;
                }

                vm = this.AllPackages.FirstOrDefault(m => m.Model == item);
                if (vm == null)
                {
                    continue;
                }

                this.SelectedPackages.Add(vm);
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
                        sortProperty = nameof(PackageViewModel.Name);
                        break;
                    case PackageSort.Publisher:
                        sortProperty = nameof(PackageViewModel.DisplayPublisherName);
                        break;
                    case PackageSort.InstallDate:
                        sortProperty = nameof(PackageViewModel.InstallDate);
                        break;
                    case PackageSort.Type:
                        sortProperty = nameof(PackageViewModel.Type);
                        break;
                    case PackageSort.Version:
                        sortProperty = nameof(PackageViewModel.Version);
                        break;
                    case PackageSort.PackageType:
                        sortProperty = nameof(PackageViewModel.DisplayPackageType);
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
                        groupProperty = nameof(PackageViewModel.DisplayPublisherName);
                        break;
                    case PackageGroup.Type:
                        groupProperty = nameof(PackageViewModel.Type);
                        break;
                    default:
                        groupProperty = null;
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
                    if (sd == null || sd.PropertyName != sortProperty || sd.Direction != (currentSortDescending ? ListSortDirection.Descending : ListSortDirection.Ascending))
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
                this.AllPackages.Add(new PackageViewModel(item));
            }
        }

        private void OnPackagesLoaded(PackagesCollectionChangedPayLoad payload)
        {
            switch (payload.Type)
            {
                case CollectionChangeType.Reset:
                    this.AllPackages.Clear();
                    foreach (var item in this.stateManager.CurrentState.Packages.VisibleItems)
                    {
                        this.AllPackages.Add(new PackageViewModel(item));
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
                        this.AllPackages.Add(new PackageViewModel(item));
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
        
        public bool IsActive { get; set; }

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

        public string SearchKey
        {
            get => this.stateManager.CurrentState.Packages.SearchKey;
            set => this.stateManager.CommandExecutor.ExecuteAsync(SetPackageFilter.CreateFrom(value));
        }

        public ObservableCollection<PackageViewModel> AllPackages { get; }

        public ICollectionView AllPackagesView { get; }
        
        public ObservableCollection<PackageViewModel> SelectedPackages { get; } = new ObservableCollection<PackageViewModel>();
        
        public Task RefreshPackages(CancellationToken cancellationToken = default(CancellationToken))
        {
            return this.stateManager.CommandExecutor.ExecuteAsync(new GetPackages(this.stateManager.CurrentState.Packages.Context), cancellationToken);
        }
    }
}
