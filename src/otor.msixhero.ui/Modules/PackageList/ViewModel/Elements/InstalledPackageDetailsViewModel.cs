using System.Collections.ObjectModel;
using System.Linq;
using otor.msixhero.lib.Domain.Appx.Manifest.Build;
using otor.msixhero.lib.Domain.Appx.Manifest.Full;
using otor.msixhero.ui.ViewModel;

namespace otor.msixhero.ui.Modules.PackageList.ViewModel.Elements
{
    public class InstalledPackageDetailsViewModel : NotifyPropertyChanged
    {
        private AppxApplicationViewModel selectedFixup;

        public InstalledPackageDetailsViewModel(AppxPackage model)
        {
            this.DisplayName = model.DisplayName;
            this.PublisherDisplayName = model.PublisherDisplayName;
            this.Version = model.Version;
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

                    this.Applications.Add(new AppxApplicationViewModel(item, model));
                }
            }
            
            this.Fixups = new ObservableCollection<AppxApplicationViewModel>(this.Applications.Where(a => a.HasPsf && a.Psf != null && (a.Psf.HasFileRedirections || a.Psf.HasOtherFixups)));
            this.selectedFixup = this.Fixups.FirstOrDefault();
            this.FixupsCount = this.Fixups.SelectMany(s => s.Psf.FileRedirections).Select(s => s.Exclusions.Count + s.Inclusions.Count).Sum();

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

        public ObservableCollection<AppxApplicationViewModel> Fixups { get; }

        public AppxApplicationViewModel SelectedFixup
        {
            get => this.selectedFixup;
            set => this.SetField(ref this.selectedFixup, value);
        }

        public BuildInfo BuildInfo { get; }

        public int FixupsCount { get; }

        public bool HasBuildInfo => this.BuildInfo != null;
    }
}
