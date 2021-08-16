// MSIX Hero
// Copyright (C) 2021 Marcin Otorowski
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// Full notice:
// https://github.com/marcinotorowski/msix-hero/blob/develop/LICENSE.md

using System;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;
using Otor.MsixHero.Appx.Packaging.Installation.Enums;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageList.ViewModels
{
    public class InstalledPackageViewModel : NotifyPropertyChanged
    {
        public InstalledPackageViewModel(InstalledPackage package)
        {
            this.Model = package;
        }

        public InstalledPackage Model { get; }

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

        public DateTime InstallDate => this.Model.InstallDate;

        public string TileColor => this.Model.TileColor;

        public string Publisher => this.Model.Publisher;

        public string ProductId => this.Model.PackageId;

        public string PackageFamilyName => this.Model.PackageFamilyName;

        public SignatureKind SignatureKind => this.Model.SignatureKind;

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
                        return "Side-loaded (Enterprise)";
                    case SignatureKind.Developer:
                        return "Side-loaded (Developer)";
                    case SignatureKind.Unsigned:
                        return "Side-loaded (unsigned)";
                    default:
                        return "Unknown";
                }
            }
        }

        public bool IsRunning
        {
            get => this.Model.IsRunning;
            set
            {
                if (this.Model.IsRunning == value)
                {
                    return;
                }

                this.Model.IsRunning = value;
                this.OnPropertyChanged();
            }
        }

        public string UserDataPath => this.Model.UserDataPath;


        public static explicit operator InstalledPackage(InstalledPackageViewModel installedPackageViewModel)
        {
            return installedPackageViewModel.Model;
        }
    }
}
