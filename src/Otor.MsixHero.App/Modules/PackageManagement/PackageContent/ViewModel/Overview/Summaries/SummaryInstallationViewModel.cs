// MSIX Hero
// Copyright (C) 2022 Marcin Otorowski
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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.Enums;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Common;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;
using Otor.MsixHero.Appx.Packaging.Services;
using Otor.MsixHero.Elevation;
using Prism.Commands;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Overview.Summaries
{
    public class SummaryInstallationViewModel : NotifyPropertyChanged, ILoadPackage
    {
        private readonly IUacElevation _uacElevation;

        private bool _isLoading;

        public SummaryInstallationViewModel(IPackageContentItemNavigation navigation, IUacElevation uacElevation, bool preciseDate)
        {
            this._uacElevation = uacElevation;
            this.PreciseDate = preciseDate;
            this.Details = new DelegateCommand(() => navigation.SetCurrentItem(PackageContentViewType.Installation));
        }

        public bool IsLoading
        {
            get => this._isLoading;
            private set => this.SetField(ref this._isLoading, value);
        }

        public ICommand Details { get; }

        public bool PreciseDate { get; }

        public async Task LoadPackage(AppxPackage model, PackageEntry installationEntry, string filePath, CancellationToken cancellationToken)
        {
            try
            {
                this.IsLoading = true;

                this.IsInstalled = true;
                
                if (installationEntry == null)
                {
                    this.FirstLine = Resources.Localization.PackageExpert_Installation_Status;
                }
                else if (installationEntry.InstallDate != default)
                {
                    if (this.PreciseDate)
                    {
                        this.FirstLine = string.Format(Resources.Localization.PackageExpert_Installation_StatusDate, installationEntry.InstallDate);
                    }
                    else
                    {
                        var dateHumanized = installationEntry.InstallDate.ToString();
                        this.FirstLine = string.Format(Resources.Localization.PackageExpert_Installation_StatusDate, dateHumanized);
                    }
                }
                else
                {
                    this.FirstLine = Resources.Localization.PackageExpert_Installation_StatusDateUnknown;
                }

                if (installationEntry == null || installationEntry.InstallDate == null)
                {
                    this.IsInstalled = false;
                    this.SecondLine = Resources.Localization.PackageExpert_Installation_NotInstalled;
                }
                else if (installationEntry.AppInstallerUri != null)
                {
                    this.SecondLine = Resources.Localization.PackageExpert_Installation_AppInstaller;
                    this.IsInstalled = true;
                }
                else
                {
                    switch (installationEntry.SignatureKind)
                    {
                        case SignatureKind.Store:
                            this.IsInstalled = true;
                            this.SecondLine = Resources.Localization.PackageExpert_Installation_Store;
                            break;
                        case SignatureKind.Unsigned:
                            this.IsInstalled = true;
                            this.SecondLine = Resources.Localization.PackageExpert_Installation_InstallType_Dev;
                            break;
                        case SignatureKind.System:
                            this.IsInstalled = true;
                            this.IsInstalled = true;
                            this.SecondLine = Resources.Localization.PackageExpert_Installation_InstallType_System;
                            break;
                        case SignatureKind.Enterprise:
                        case SignatureKind.Developer:
                            this.IsInstalled = true;
                            this.SecondLine = Resources.Localization.PackageExpert_Installation_InstallType_SideLoading;
                            break;
                        case SignatureKind.Unknown:
                            this.IsInstalled = false;
                            this.SecondLine = Resources.Localization.PackageExpert_Installation_InstallType_Unknown;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                
                this.ThirdLine = this.AddOnsCount > 1 ? string.Format(Resources.Localization.PackageExpert_Installation_AddOnX, this.AddOnsCount) : Resources.Localization.PackageExpert_Installation_AddOn1;

                this.AddOnsCount = await this.GetAddOnsCount(model, cancellationToken).ConfigureAwait(false);

                this.OnPropertyChanged(null);
            }
            finally
            {
                this.IsLoading = false;
            }
        }

        public string FirstLine { get; private set; }

        public bool IsInstalled { get; private set; }

        public string SecondLine { get; private set; }

        public string ThirdLine { get; private set; }

        public int AddOnsCount { get; private set; }

        public bool HasAddOns => this.AddOnsCount > 0;
        
        private async Task<int> GetAddOnsCount(AppxPackage package, CancellationToken cancellationToken = default)
        {
            var results = await this._uacElevation.AsHighestAvailable<IAppxPackageQueryService>().GetModificationPackages(package.FullName, PackageFindMode.Auto, cancellationToken).ConfigureAwait(false);
            return results.Count;
        }
    }
}
