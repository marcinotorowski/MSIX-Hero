using Otor.MsixHero.Appx.Packaging.Manifest.Entities;
using Otor.MsixHero.Ui.ViewModel;

namespace Otor.MsixHero.Ui.Modules.Common.PackageContent.ViewModel.Elements
{
    public class PackageDependencyViewModel : NotifyPropertyChanged
    {
        private readonly AppxPackageDependency model;

        public PackageDependencyViewModel(AppxPackageDependency model)
        {
            this.model = model;
        }

        public string Version => this.model.Dependency?.Version?.ToString() ?? this.model.Version;

        public string DisplayName => this.model.Dependency?.DisplayName ?? this.model.Name;

        public string DisplayPublisherName => this.model.Dependency?.PublisherDisplayName ?? this.model.Publisher;

        public string Name => this.model.Name;

        public string Publisher => this.model.Publisher;
    }
}