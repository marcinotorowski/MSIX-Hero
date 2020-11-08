using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;

namespace Otor.MsixHero.App.Modules.Packages.ViewModels.PackageExpert.Items
{
    public class PackageDependencyViewModel : NotifyPropertyChanged
    {
        private readonly AppxPackageDependency model;

        public PackageDependencyViewModel(AppxPackageDependency model)
        {
            this.model = model;
        }

        public string Version => this.model.Dependency?.Version ?? this.model.Version;

        public string DisplayName => this.model.Dependency?.DisplayName ?? this.model.Name;

        public string DisplayPublisherName => this.model.Dependency?.PublisherDisplayName ?? this.model.Publisher;

        public string Name => this.model.Name;

        public string Publisher => this.model.Publisher;
    }
}