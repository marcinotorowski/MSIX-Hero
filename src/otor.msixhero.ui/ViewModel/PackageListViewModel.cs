using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using otor.msihero.lib;

namespace MSI_Hero.ViewModel
{
    public class PackageListViewModel : NotifyPropertyChanged
    {
        private bool isSelected, isLoading;
        private string filterString;
        private PackageViewModel selectedPackage;
        private bool showStoreApps, showSystemApps, showSideLoadedApps;

        public PackageListViewModel(AppxPackageManager packageManager)
        {
            this.PackageManager = packageManager;
            this.showSideLoadedApps = true;

            this.AllPackages = new ObservableCollection<PackageViewModel>();
            this.AllPackagesView = CollectionViewSource.GetDefaultView(this.AllPackages);
            this.AllPackagesView.Filter += this.FilterPackage;
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

        public bool IsLoading
        {
            get => this.isLoading;
            private set => this.SetField(ref this.isLoading, value);
        }

        public AppxPackageManager PackageManager { get; }

        public async Task RefreshPackages()
        {
            try
            {
                this.IsLoading = true;
                await Task.Delay(100);

                var task = Task.Run(() => this.PackageManager.GetPackages(), CancellationToken.None);
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
            finally
            {
                this.IsLoading = false;
            }
        }
    }
}
