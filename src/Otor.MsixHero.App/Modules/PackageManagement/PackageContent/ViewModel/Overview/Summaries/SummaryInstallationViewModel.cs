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

using System.Threading.Tasks;
using System.Windows.Input;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.Enums;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Common;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities.Sources;
using Prism.Commands;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Overview.Summaries
{
    public class SummaryInstallationViewModel : NotifyPropertyChanged, ILoadPackage
    {
        public SummaryInstallationViewModel(IPackageContentItemNavigation navigation)
        {
            this.Details = new DelegateCommand(() => navigation.SetCurrentItem(PackageContentViewType.Installation));
        }

        public ICommand Details { get; }

        public Task LoadPackage(AppxPackage model, string filePath)
        {
            if (model.Source is NotInstalledSource)
            {
                this.FirstLine = "Installation status";
            }
            else if (model.Source.InstallDate != default)
            {
                this.FirstLine = "Installed on **" + model.Source.InstallDate + "**";
            }
            else
            {
                this.FirstLine = "Installed";
            }

            if (model.Source is NotInstalledSource)
            {
                this.SecondLine = "Not installed";
            }
            else if (model.Source is StorePackageSource)
            {
                this.SecondLine = "from Microsoft Store";
            }
            else if (model.Source is AppInstallerPackageSource)
            {
                this.SecondLine = "from .appinstaller file";
            }
            else if (model.Source is SystemSource)
            {
                this.SecondLine = "as system application";
            }
            else if (model.Source is DeveloperSource)
            {
                this.SecondLine = "in development mode";
            }
            else if (model.Source is StandardSource)
            {
                this.SecondLine = "via side-loading";
            }
            else
            {
                this.SecondLine = "Unknown";
            }
            
            this.OnPropertyChanged(null);

            return Task.CompletedTask;
        }
        
        public string FirstLine { get; private set; }

        public string SecondLine { get; private set; }
    }
}
