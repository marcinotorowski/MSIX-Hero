using System;
using System.Linq;
using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;
using Otor.MsixHero.Appx.Packaging.Installation.Enums;
using Otor.MsixHero.Ui.Hero;
using Otor.MsixHero.Ui.Hero.Commands.Packages;
using Otor.MsixHero.Ui.Modules.VolumeManager.ViewModel.Elements;

namespace Otor.MsixHero.Ui.Modules.PackageList.ViewModel.Elements
{
    public class InstalledPackageViewModel : SelectableViewModel<InstalledPackage>
    {
        private readonly IMsixHeroApplication parent;

        public InstalledPackageViewModel(IMsixHeroApplication parent, InstalledPackage package, bool isSelected = false) : base(package, isSelected)
        {
            this.parent = parent;
        }

        public bool IsAddon => this.Model.IsOptional;

        public string Description => this.Model.Description;

        public string DisplayName => this.Model.DisplayName;

        public bool IsProvisioned => this.Model.IsProvisioned;

        public string Name => this.Model.Name;

        public string Version => this.Model.Version.ToString();

        public string Architecture => this.Model.Architecture;

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

        
        public static explicit operator InstalledPackage(InstalledPackageViewModel installedPackageViewModel)
        {
            return installedPackageViewModel.Model;
        }

        protected override bool TrySelect()
        {
            var selected = parent.ApplicationState.Packages.SelectedPackages.Select(p => p.ManifestLocation).Union(new [] { this.ManifestLocation });
            this.parent.CommandExecutor.Invoke(this, new SelectPackagesCommand(selected));
            // this.parent.NavigateToSelection();
            return true;
        }

        protected override bool TryUnselect()
        {
            var selected = parent.ApplicationState.Packages.SelectedPackages.Select(p => p.ManifestLocation).Except(new[] { this.ManifestLocation });
            this.parent.CommandExecutor.Invoke(this, new SelectPackagesCommand(selected));
            // this.parent.NavigateToSelection();
            return true;
        }
    }
}
