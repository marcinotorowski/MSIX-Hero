using System.Collections.ObjectModel;
using otor.msixhero.lib.Domain.Appx.Manifest.Build;
using otor.msixhero.lib.Domain.Appx.Manifest.Full;
using otor.msixhero.ui.ViewModel;

namespace otor.msixhero.ui.Modules.PackageList.ViewModel
{
    public class InstalledPackageDetailsViewModel : NotifyPropertyChanged
    {
        public InstalledPackageDetailsViewModel(AppxPackage model)
        {
            this.DisplayName = model.DisplayName;
            this.PublisherDisplayName = model.PublisherDisplayName;
            this.Version = model.Version.ToString();
            this.Logo = model.Logo;
            this.FamilyName = model.FamilyName;

            this.OperatingSystemDependencies = new ObservableCollection<OperatingSystemDependencyViewModel>();
            this.Applications = new ObservableCollection<AppxApplicationViewModel>();
            this.PackageDependencies = new ObservableCollection<PackageDependencyViewModel>();
            this.Addons = new ObservableCollection<InstalledPackageDetailsViewModel>();

            if (model.OperatingSystemDependencies != null)
            {
                foreach (var item in model.OperatingSystemDependencies)
                {
                    this.OperatingSystemDependencies.Add(new OperatingSystemDependencyViewModel(item));
                }
            }

            if (model.PackageDependencies != null)
            {
                foreach (var item in model.PackageDependencies)
                {
                    this.PackageDependencies.Add(new PackageDependencyViewModel(item));
                }
            }

            if (model.Applications != null)
            {
                foreach (var item in model.Applications)
                {
                    if (string.IsNullOrEmpty(this.TileColor))
                    {
                        this.TileColor = item.BackgroundColor;
                    }

                    this.Applications.Add(new AppxApplicationViewModel(item));
                }
            }

            if (model.Addons != null)
            {
                foreach (var item in model.Addons)
                {
                    this.Addons.Add(new InstalledPackageDetailsViewModel(item));
                }
            }

            this.BuildInfo = model.BuildInfo;

            if (string.IsNullOrEmpty(this.TileColor))
            {
                this.TileColor = "#666666";
            }
        }

        public string PublisherDisplayName { get; }

        public string TileColor { get; }

        public string DisplayName { get; }

        public byte[] Logo { get; }

        public string FamilyName { get; }

        public string Version { get; }
        
        public ObservableCollection<OperatingSystemDependencyViewModel> OperatingSystemDependencies { get; }

        public ObservableCollection<PackageDependencyViewModel> PackageDependencies { get; }
        
        public ObservableCollection<InstalledPackageDetailsViewModel> Addons { get; }
        
        public ObservableCollection<AppxApplicationViewModel> Applications { get; }

        public BuildInfo BuildInfo { get; }

        public bool HasBuildInfo => this.BuildInfo != null;
    }
}
