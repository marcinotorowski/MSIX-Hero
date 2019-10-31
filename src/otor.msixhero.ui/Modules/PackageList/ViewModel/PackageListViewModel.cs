using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.PowerShell.Commands;
using otor.msixhero.lib;
using otor.msixhero.lib.BusinessLayer.Commands;
using otor.msixhero.lib.BusinessLayer.Commands.Grid;
using otor.msixhero.lib.BusinessLayer.Events;
using otor.msixhero.lib.BusinessLayer.Infrastructure;
using otor.msixhero.lib.BusinessLayer.Models;
using otor.msixhero.lib.BusinessLayer.State.Enums;
using otor.msixhero.ui.Commands.RoutedCommand;
using otor.msixhero.ui.Helpers;
using otor.msixhero.ui.ViewModel;
using Prism;
using Prism.Events;
using Prism.Regions;
using DelegateCommand = Prism.Commands.DelegateCommand;

namespace otor.msixhero.ui.Modules.PackageList.ViewModel
{
    public class PackageListViewModel : NotifyPropertyChanged, INavigationAware, IActiveAware
    {
        private readonly IApplicationStateManager stateManager;
        private PackageViewModel selectedPackage;
        private bool isActive;
        private ICommand findUsers;
        private bool disableSelectionPropagation;

        public PackageListViewModel(IApplicationStateManager stateManager)
        {
            this.stateManager = stateManager;

            this.AllPackages = new ObservableCollection<PackageViewModel>();
            this.AllPackagesView = CollectionViewSource.GetDefaultView(this.AllPackages);
            this.SetSortingAndGrouping();

            stateManager.EventAggregator.GetEvent<PackagesFilterChanged>().Subscribe(this.OnPackageFilterChanged);
            stateManager.EventAggregator.GetEvent<PackagesLoaded>().Subscribe(this.OnPackagesLoaded, ThreadOption.UIThread);
            stateManager.EventAggregator.GetEvent<PackagesVisibilityChanged>().Subscribe(this.OnPackagesVisibilityChanged);
            stateManager.EventAggregator.GetEvent<PackagesSelectionChanged>().Subscribe(this.OnPackagesSelectionChanged, ThreadOption.UIThread);
            stateManager.EventAggregator.GetEvent<PackageGroupAndSortChanged>().Subscribe(this.OnGroupAndSortChanged);
        }

        public AsyncProperty<SelectionDetailsViewModel> SelectedPackageExtendedInfo { get; } = new AsyncProperty<SelectionDetailsViewModel>();

        private bool FindUsersCanExecute()
        {
            return true;
        }

        private async Task FindUsersExecuteAsync()
        {
            var list = await this.stateManager.CommandExecutor.GetExecuteAsync(new GetUsersOfPackage(this.selectedPackage.ProductId));
            this.SelectedPackage.Users = list;
        }

        private void OnPackagesSelectionChanged(PackagesSelectionChangedPayLoad selectionInfo)
        {
            this.selectedPackage = this.AllPackages.FirstOrDefault(app => this.stateManager.CurrentState.Packages.SelectedItems.Contains(app.Model));
            this.OnPropertyChanged(nameof(SelectedPackage));
            this.OnPropertyChanged(nameof(IsSelected));

#pragma warning disable 4014
            if (selectedPackage == null)
            {
                this.SelectedPackageExtendedInfo.Load(Task.FromResult((SelectionDetailsViewModel)null));
            }
            else
            {
                try
                {
                    this.SelectedPackageExtendedInfo.Load(this.GetSelectionDetails(selectedPackage.Model, this.stateManager.CurrentState.IsElevated || this.stateManager.CurrentState.HasSelfElevated));
                }
                catch (Exception)
                {
                    this.SelectedPackageExtendedInfo.Load(this.GetSelectionDetails(selectedPackage.Model, false));
                }
#pragma warning restore 4014   
            }
        }

        private async Task<SelectionDetailsViewModel> GetSelectionDetails(Package package, bool forceElevation)
        {
            var stateDetails = await this.stateManager.CommandExecutor.GetExecuteAsync(new GetSelectionDetails(package, forceElevation)).ConfigureAwait(false);
            return new SelectionDetailsViewModel(stateDetails);
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

        private void OnPackagesLoaded(PackageContext context)
        {
            this.disableSelectionPropagation = true;
            try
            {
                this.AllPackages.Clear();
                foreach (var item in this.stateManager.CurrentState.Packages.VisibleItems)
                {
                    this.AllPackages.Add(new PackageViewModel(item));
                }
            }
            finally
            {
                this.disableSelectionPropagation = false;
            }
        }

        private void OnPackageFilterChanged(PackagesFilterChangedPayload filter)
        {
            if (filter.NewSearchKey != filter.OldSearchKey)
            {
                this.OnPropertyChanged(nameof(SearchKey));
            }
        }
        
        public bool IsActive
        {
            get
            {
                return this.isActive;
            }
            set
            {
                this.isActive = value;
                var iac = this.IsActiveChanged;
                if (iac != null)
                {
                    iac(this, new EventArgs());
                }

#pragma warning disable 4014
                this.RefreshPackages();
#pragma warning restore 4014
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

        public string SearchKey
        {
            get => this.stateManager.CurrentState.Packages.SearchKey;
            set => this.stateManager.CommandExecutor.ExecuteAsync(SetPackageFilter.CreateFrom(value));
        }

        public ObservableCollection<PackageViewModel> AllPackages { get; }

        public ICollectionView AllPackagesView { get; }
        
        public PackageViewModel SelectedPackage
        {
            get => this.selectedPackage;
            set
            {
                if (this.disableSelectionPropagation)
                {
                    return;
                }

                if (value == null)
                {
                    this.stateManager.CommandExecutor.ExecuteAsync(SelectPackages.CreateEmpty());
                }
                else
                {
                    this.stateManager.CommandExecutor.ExecuteAsync(new SelectPackages(value.Model));
                }
            }
        }

        public bool IsSelected
        {
            get => this.selectedPackage != null;
        }

        public ICommand FindUsers
        {
            get 
            {
                return this.findUsers ?? (this.findUsers = new DelegateCommand(() =>
                {
#pragma warning disable 4014
                    this.SelectedPackageExtendedInfo.Load(this.GetSelectionDetails( this.SelectedPackage.Model, true));
#pragma warning restore 4014
                }));
            }
        }

        public Task RefreshPackages(CancellationToken cancellationToken = default(CancellationToken))
        {
            return this.stateManager.CommandExecutor.ExecuteAsync(new GetPackages(this.stateManager.CurrentState.Packages.Context), cancellationToken);
        }
    }
}
