using System;
using System.Collections.ObjectModel;
using System.Linq;
using otor.msixhero.lib.BusinessLayer.Helpers;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Appx.Packages;
using otor.msixhero.lib.Domain.Commands.Packages.Grid;
using otor.msixhero.ui.Modules.VolumeManager.ViewModel.Elements;
using otor.msixhero.ui.ViewModel;

namespace otor.msixhero.ui.Modules.PackageList.ViewModel.Elements
{
    public class InstalledPackageViewModel : SelectableViewModel<InstalledPackage>
    {
        public InstalledPackageViewModel(InstalledPackage package, IApplicationStateManager stateManager, bool isSelected = false) : base(package, stateManager, isSelected)
        {
        }

        public string Description => this.Model.Description;

        public string DisplayName => this.Model.DisplayName;

        public bool IsProvisioned => this.Model.IsProvisioned;

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

        
        public static explicit operator InstalledPackage(InstalledPackageViewModel installedPackageViewModel)
        {
            return installedPackageViewModel.Model;
        }

        protected override bool TrySelect()
        {
            if (!this.StateManager.CurrentState.Packages.SelectedItems.Contains(this.Model))
            {
                this.StateManager.CommandExecutor.Execute(new SelectPackages(this.Model, SelectionMode.AddToSelection));
                return true;
            }

            return false;
        }

        protected override bool TryUnselect()
        {
            if (this.StateManager.CurrentState.Packages.SelectedItems.Contains(this.Model))
            {
                this.StateManager.CommandExecutor.Execute(new SelectPackages(this.Model, SelectionMode.RemoveFromSelection));
                return true;
            }

            return false;
        }
    }
}
