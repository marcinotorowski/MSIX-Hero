using System;
using System.Collections.ObjectModel;
using otor.msixhero.lib.BusinessLayer.Helpers;
using otor.msixhero.lib.Domain.Appx.Packages;

namespace otor.msixhero.ui.ViewModel
{
    public class InstalledPackageViewModel : NotifyPropertyChanged
    {
        public InstalledPackageViewModel(InstalledPackage package)
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

        public MsixPackageType PackageType => this.Model.PackageType;

        public string DisplayPackageType => PackageTypeConverter.GetPackageTypeStringFrom(this.Model.PackageType);

        public string Image => this.Model.Image;

        public DateTime InstallDate { get => this.Model.InstallDate; }

        public string TileColor => this.Model.TileColor;

        public string Publisher => this.Model.Publisher;

        public string ProductId => this.Model.PackageId;

        public string PackageFamilyName => this.Model.PackageFamilyName;
        
        public SignatureKind SignatureKind
        {
            get => this.Model.SignatureKind;
        }

        public string Type
        {
            get
            {
                switch (this.SignatureKind)
                {
                    case SignatureKind.System:
                        return "System app";
                    case SignatureKind.Store:
                        return "Store app";
                    case SignatureKind.Enterprise:
                        return "Sideloaded (Enterprise)";
                    case SignatureKind.Developer:
                        return "Sideloaded (Developer)";
                    case SignatureKind.Unsigned:
                        return "Sideloaded (unsigned)";
                    default:
                        return "Unknown";
                }
            }
        }

        public string UserDataPath
        {
            get => this.Model.UserDataPath;
        }

        public InstalledPackage Model { get; }

        public ObservableCollection<OperatingSystemViewModel> TargetOperatingSystems { get; }

        
        public static explicit operator InstalledPackage(InstalledPackageViewModel installedPackageViewModel)
        {
            return installedPackageViewModel.Model;
        }
    }
}
