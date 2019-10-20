using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using MSI_Hero.Modules.Installed.Events;
using MSI_Hero.Services;
using MSI_Hero.ViewModel;
using otor.msihero.lib;
using otor.msixhero.lib;
using Prism;
using Prism.Events;
using Prism.Regions;

namespace MSI_Hero.Modules.Installed.ViewModel
{
    public class InstalledViewModel : NotifyPropertyChanged, INavigationAware, IActiveAware
    {
        private readonly IBusyManager busyManager;
        private bool isSelected;
        private string filterString;
        private bool allUsers;
        private PackageViewModel selectedPackage;
        private bool showStoreApps, showSystemApps, showSideLoadedApps;
        private bool isActive;

        public InstalledViewModel(IAppxPackageManager packageManager, IEventAggregator eventAggregator, IBusyManager busyManager)
        {
            this.busyManager = busyManager;
            this.PackageManager = packageManager;
            this.showSideLoadedApps = true;

            this.AllPackages = new ObservableCollection<PackageViewModel>();
            this.AllPackagesView = CollectionViewSource.GetDefaultView(this.AllPackages);
            this.AllPackagesView.Filter += this.FilterPackage;

            this.allUsers = UserHelper.IsAdministrator();

            eventAggregator.GetEvent<InstalledListRefreshRequestEvent>().Subscribe(this.OnInstalledListRefreshRequest);
        }

        private async void OnInstalledListRefreshRequest(bool obj)
        {
            await this.RefreshPackages().ConfigureAwait(false);
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

        public bool AllUsers
        {
            get => this.allUsers;
            set
            {
                this.SetField(ref this.allUsers, value);
#pragma warning disable 4014
                this.RefreshPackages();
#pragma warning restore 4014
            }
        }

        public bool ShowSideLoadedApps
        {
            get => this.showSideLoadedApps;
            set
            {
                this.SetField(ref this.showSideLoadedApps, value);
                this.AllPackagesView.Refresh();
            }
        }

        public bool ShowStoreApps
        {
            get => this.showStoreApps;
            set
            {
                this.SetField(ref this.showStoreApps, value);
                this.AllPackagesView.Refresh();
            }
        }

        public bool ShowSystemApps
        {
            get => this.showSystemApps;
            set
            {
                this.SetField(ref this.showSystemApps, value);
                this.AllPackagesView.Refresh();
            }
        }

        public string FilterString
        {
            get => this.filterString;
            set
            {
                this.SetField(ref this.filterString, value);
                this.AllPackagesView.Refresh();
            }
        }

        private bool FilterPackage(object obj)
        {
            var pkg = obj as PackageViewModel;
            if (pkg == null)
            {
                return false;
            }

            switch (pkg.SignatureKind)
            {
                case SignatureKind.Store:
                    if (!this.showStoreApps)
                    {
                        return false;
                    }

                    break;
                case SignatureKind.System:
                    if (!this.showSystemApps)
                    {
                        return false;
                    }

                    break;
                case SignatureKind.Developer:
                case SignatureKind.Enterprise:
                case SignatureKind.None:
                    if (!this.showSideLoadedApps)
                    {
                        return false;
                    }

                    break;
            }

            if (!string.IsNullOrEmpty(this.filterString) && pkg.DisplayName.IndexOf(this.FilterString, StringComparison.OrdinalIgnoreCase) == -1 && pkg.DisplayPublisherName.IndexOf(this.FilterString, StringComparison.OrdinalIgnoreCase) == -1)
            {
                return false;
            }

            return true;
        }

        public ObservableCollection<PackageViewModel> AllPackages { get; }

        public ObservableCollection<PackageViewModel> SelectedPackages { get; } = new ObservableCollection<PackageViewModel>();

        public ICollectionView AllPackagesView { get; }

        public PackageViewModel SelectedPackage
        {
            get => this.selectedPackage;
            set
            {
                this.SetField(ref this.selectedPackage, value);
                this.IsSelected = value != null;
            }
        }

        public bool IsSelected
        {
            get => this.isSelected;
            private set => this.SetField(ref this.isSelected, value);
        }

        public IAppxPackageManager PackageManager { get; }

        public Task RefreshPackages()
        {
            return this.busyManager.Execute(this.RefreshPackagesList);
        }

        private async Task RefreshPackagesList(IBusyContext busyContext)
        {
            try
            {
                busyContext.Message = "Loading packages";
                busyContext.Progress = 50;

                await Task.Delay(100);

                var task = Task.Run(
                    () => this.PackageManager.GetPackages(this.allUsers
                        ? PackageFindMode.AllUsers
                        : PackageFindMode.CurrentUser), CancellationToken.None);
                var result = await task;

                await Task.Delay(100);

                var currentSelection = new HashSet<string>(this.SelectedPackages.Select(sp => sp.ProductId).ToList());

                if (this.SelectedPackage != null)
                {
                    currentSelection.Add(this.SelectedPackage.ProductId);
                }

                this.AllPackages.Clear();
                this.SelectedPackages.Clear();

                foreach (var item in result)
                {
                    var pkgViewModel = new PackageViewModel(item);
                    this.AllPackages.Add(pkgViewModel);
                    if (!currentSelection.Contains(item.ProductId))
                    {
                        continue;
                    }

                    this.SelectedPackage = pkgViewModel;
                    this.SelectedPackages.Add(pkgViewModel);
                }
            }
            catch (UnauthorizedAccessException)
            {
                // todo: Right dialog
                MessageBox.Show("Access denied. Do you want to run this command as a local administrator?", "Access denied", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
            }
        }
    }
}
