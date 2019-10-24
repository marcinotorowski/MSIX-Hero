using otor.msihero.lib;

namespace MSI_Hero.ViewModel
{
    public class PackageViewModel : NotifyPropertyChanged
    {
        public PackageViewModel(Package package)
        {
            this.Model = package;
        }

        public string Description => this.Model.Description;

        public string DisplayName => this.Model.DisplayName;

        public string Name => this.Model.Name;

        public string Version => this.Model.Version.ToString();

        public string DisplayPublisherName => this.Model.DisplayPublisherName;

        public string InstallLocation => this.Model.InstallLocation;

        public string ManifestLocation => this.Model.ManifestLocation;

        public string Image => this.Model.Image;

        public string TileColor => this.Model.TileColor;

        public string Publisher => this.Model.Publisher;

        public string ProductId => this.Model.ProductId;

        public string PackageFamilyName => this.Model.PackageFamilyName;
        
        public SignatureKind SignatureKind
        {
            get => this.Model.SignatureKind;
        }

        public string UserDataPath
        {
            get => this.Model.UserDataPath;
        }

        public Package Model { get; }

        public static explicit operator Package(PackageViewModel packageViewModel)
        {
            return packageViewModel.Model;
        }
    }
}
