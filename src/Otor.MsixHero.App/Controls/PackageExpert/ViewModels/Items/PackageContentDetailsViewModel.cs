using System.Collections.ObjectModel;
using System.Linq;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities.Build;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities.Sources;

namespace Otor.MsixHero.App.Controls.PackageExpert.ViewModels.Items
{
    public class PackageContentDetailsViewModel : NotifyPropertyChanged
    {
        private AppxApplicationViewModel selectedFixup;

        public PackageContentDetailsViewModel(AppxPackage model, string filePath = null)
        {
            this.Model = model;
            this.DisplayName = model.DisplayName;
            this.Description = model.Description;
            this.Publisher = model.Publisher;
            this.FamilyName = model.FamilyName;
            this.Architecture = model.ProcessorArchitecture.ToString();
            this.PackageFullName = model.FullName;
            this.PublisherDisplayName = model.PublisherDisplayName;
            this.Version = model.Version;
            this.Logo = model.Logo;
            
            this.OperatingSystemDependencies = new ObservableCollection<OperatingSystemDependencyViewModel>();
            this.Applications = new ObservableCollection<AppxApplicationViewModel>();
            this.PackageDependencies = new ObservableCollection<PackageDependencyViewModel>();
            
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

            this.HasOperatingSystemDependencies = this.OperatingSystemDependencies.Any();
            this.HasPackageDependencies = this.PackageDependencies.Any();

            this.ScriptsCount = 0;

            if (model.Applications != null)
            {
                foreach (var item in model.Applications)
                {
                    if (string.IsNullOrEmpty(this.TileColor))
                    {
                        this.TileColor = item.BackgroundColor;
                    }

                    this.Applications.Add(new AppxApplicationViewModel(item, model));
                    this.ScriptsCount += item.Psf?.Scripts?.Count ?? 0;
                }
            }
            
            this.Fixups = new ObservableCollection<AppxApplicationViewModel>(this.Applications.Where(a => a.HasPsf && a.Psf != null && (a.Psf.HasFileRedirections || a.Psf.HasTracing || a.Psf.HasOtherFixups)));
            this.selectedFixup = this.Fixups.FirstOrDefault();
            
            // 1) fixup count is the sum of all individual file redirections...
            this.FixupsCount = this.Fixups.SelectMany(s => s.Psf.FileRedirections).Select(s => s.Exclusions.Count + s.Inclusions.Count).Sum();
            
            // 2) plus additionally number of apps that have tracing
            this.FixupsCount += this.Applications.Count(a => a.HasPsf && a.Psf.HasTracing);
            
            this.BuildInfo = model.BuildInfo;

            if (string.IsNullOrEmpty(this.TileColor))
            {
                this.TileColor = "#666666";
            }

            this.Capabilities = new CapabilitiesViewModel(model.Capabilities);
            this.PackageIntegrity = model.PackageIntegrity;
            this.RootDirectory = filePath;
        }

        public string RootDirectory { get; }

        public AppxPackage Model { get; private set; }

        public bool HasAppInstallerUri => this.Model.Source is AppInstallerPackageSource;

        public bool PackageIntegrity { get; }

        public CapabilitiesViewModel Capabilities { get; }

        public string PackageFullName { get; }

        public string Description { get; }

        public string PublisherDisplayName { get; }

        public string Publisher { get; }

        public string TileColor { get; }

        public string DisplayName { get; }

        public byte[] Logo { get; }

        public string FamilyName { get; }

        public string Architecture { get; }

        public string Version { get; }
        
        public ObservableCollection<OperatingSystemDependencyViewModel> OperatingSystemDependencies { get; }

        public bool HasOperatingSystemDependencies { get; }

        public bool HasPackageDependencies { get; }

        public ObservableCollection<PackageDependencyViewModel> PackageDependencies { get; }
        
        public ObservableCollection<AppxApplicationViewModel> Applications { get; }

        public ObservableCollection<AppxApplicationViewModel> Fixups { get; }

        public AppxApplicationViewModel SelectedFixup
        {
            get => this.selectedFixup;
            set => this.SetField(ref this.selectedFixup, value);
        }

        public BuildInfo BuildInfo { get; }

        public int FixupsCount { get; }
        
        public int ScriptsCount { get; }

        public bool HasBuildInfo => this.BuildInfo != null;
    }
}
