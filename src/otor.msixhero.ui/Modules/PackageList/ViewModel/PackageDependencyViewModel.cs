using otor.msixhero.lib.BusinessLayer.Models.Manifest;
using otor.msixhero.ui.ViewModel;

namespace otor.msixhero.ui.Modules.PackageList.ViewModel
{
    public class PackageDependencyViewModel : NotifyPropertyChanged
    {
        private readonly PackageDependency model;
        private readonly string displayName;
        private readonly string displayPublisherName;

        public PackageDependencyViewModel(PackageDependency model, string displayName = null, string displayPublisherName = null)
        {
            this.model = model;
            this.displayName = displayName;
            this.displayPublisherName = displayPublisherName;
        }

        public string Version => this.model.Version;

        public string DisplayName => this.displayName ?? this.model.Name;

        public string DisplayPublisherName => this.displayPublisherName ?? this.model.Publisher;

        public string Name => this.model.Name;

        public string Publisher => this.model.Publisher;

        public bool HasDisplayName => this.displayName != null;

        public bool HasDisplayPublisherName => this.displayPublisherName != null;
    }
}