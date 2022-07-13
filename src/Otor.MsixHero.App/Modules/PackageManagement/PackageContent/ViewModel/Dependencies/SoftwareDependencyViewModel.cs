using Otor.MsixHero.Appx.Packaging.Manifest.Entities;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Dependencies
{
    public class SoftwareDependencyViewModel
    {
        private readonly AppxPackageDependency _model;

        public SoftwareDependencyViewModel(AppxPackageDependency model)
        {
            this._model = model;
        }

        public string Version => this._model.Dependency?.Version ?? this._model.Version;

        public string DisplayName => this._model.Dependency?.DisplayName ?? this._model.Name;

        public string DisplayPublisherName => this._model.Dependency?.PublisherDisplayName ?? this._model.Publisher;

        public string Name => this._model.Name;

        public string Publisher => this._model.Publisher;
    }
}
