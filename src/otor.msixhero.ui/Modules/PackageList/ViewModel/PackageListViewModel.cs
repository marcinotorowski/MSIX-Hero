using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using otor.msixhero.lib.BusinessLayer.Commands;
using otor.msixhero.lib.BusinessLayer.Events;
using otor.msixhero.lib.BusinessLayer.Infrastructure;
using otor.msixhero.lib.BusinessLayer.Models;
using otor.msixhero.lib.BusinessLayer.State.Enums;
using otor.msixhero.ui.Commands.RoutedCommand;
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
        private readonly Lazy<ICommand> findUsers;
        private PackageViewModel selectedPackage;
        private bool isActive;

        public PackageListViewModel(IApplicationStateManager stateManager)
        {
            this.stateManager = stateManager;

            this.AllPackages = new ObservableCollection<PackageViewModel>();
            this.AllPackagesView = CollectionViewSource.GetDefaultView(this.AllPackages);
            //this.AllPackagesView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(PackageViewModel.DisplayPublisherName)));

            stateManager.EventAggregator.GetEvent<PackagesFilterChanged>().Subscribe(this.OnPackageFilterChanged, ThreadOption.UIThread);
            stateManager.EventAggregator.GetEvent<PackagesLoadedEvent>().Subscribe(this.OnPackagesLoaded, ThreadOption.UIThread);
            stateManager.EventAggregator.GetEvent<PackagesVisibilityChanged>().Subscribe(this.OnPackagesVisibilityChanged, ThreadOption.UIThread);
            stateManager.EventAggregator.GetEvent<PackagesSelectionChanged>().Subscribe(this.OnPackagesSelectionChanged, ThreadOption.UIThread);

            this.findUsers = new Lazy<ICommand>(() => new DelegateCommand(() => this.FindUsersExecuteAsync(), this.FindUsersCanExecute));
        }

        private bool FindUsersCanExecute()
        {
            return true;
        }

        private async Task FindUsersExecuteAsync()
        {
            var list = await this.stateManager.CommandExecutor.ExecuteAsync<List<User>>(new GetUsersOfPackage(this.selectedPackage.ProductId));
            this.SelectedPackage.Users = list;
        }

        private void OnPackagesSelectionChanged(PackagesSelectionChangedPayLoad selectionInfo)
        {
            this.selectedPackage = this.AllPackages.FirstOrDefault(app => this.stateManager.CurrentState.Packages.SelectedItems.Contains(app.Model));
            this.OnPropertyChanged(nameof(SelectedPackage));
            this.OnPropertyChanged(nameof(IsSelected));
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
            this.AllPackages.Clear();
            foreach (var item in this.stateManager.CurrentState.Packages.VisibleItems)
            {
                this.AllPackages.Add(new PackageViewModel(item));
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
            get => this.findUsers.Value;
        }

        public Task RefreshPackages(CancellationToken cancellationToken = default(CancellationToken))
        {
            return this.stateManager.CommandExecutor.ExecuteAsync(new GetPackages(this.stateManager.CurrentState.Packages.Context), cancellationToken);
        }
    }
}
