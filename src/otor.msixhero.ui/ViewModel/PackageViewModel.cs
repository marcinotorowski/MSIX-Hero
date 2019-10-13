using otor.msihero.lib;

namespace MSI_Hero.ViewModel
{
    public class PackageViewModel : NotifyPropertyChanged
    {
        private readonly Package package;

        public PackageViewModel(Package package)
        {
            this.package = package;
        }

        public string Description => this.package.Description;

        public string DisplayName => this.package.DisplayName;

        public string Version => this.package.Version.ToString();

        public string DisplayPublisherName => this.package.DisplayPublisherName;

        public string InstallLocation => this.package.InstallLocation;

        public string ManifestLocation => this.package.ManifestLocation;

        public string Image => this.package.Image;

        public string ProductId => this.package.ProductId;

        public SignatureKind SignatureKind
        {
            get => this.package.SignatureKind;
        }

        public static explicit operator Package(PackageViewModel packageViewModel)
        {
            return packageViewModel.package;
        }
    }
}
