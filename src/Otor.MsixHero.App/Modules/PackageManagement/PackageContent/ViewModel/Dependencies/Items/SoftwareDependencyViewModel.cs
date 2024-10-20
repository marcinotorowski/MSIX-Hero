using Otor.MsixHero.Appx.Reader.Manifest.Entities;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Dependencies.Items
{
    public class SoftwareDependencyViewModel
    {
        private readonly AppxPackageDependency _model;

        public SoftwareDependencyViewModel(AppxPackageDependency model)
        {
            _model = model;
        }

        public string Version => _model.Dependency?.Version ?? _model.Version;

        public string DisplayName => _model.Dependency?.DisplayName ?? _model.Name;

        public string DisplayPublisherName => _model.Dependency?.PublisherDisplayName ?? _model.Publisher;

        public string Name => _model.Name;

        public AppxPackageDependency Model => this._model;

        public string Publisher => _model.Publisher;
    }
}
