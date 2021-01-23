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

using System.Collections;
using System.Linq;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;
using Prism.Regions;

namespace Otor.MsixHero.App.Modules.PackageManagement.Details.ViewModels
{
    public class PackagesSingleDetailsViewModel : NotifyPropertyChanged, INavigationAware
    {
        private string filePath;

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (navigationContext.Parameters.First().Value is IEnumerable pkgs)
            {
                this.FilePath = pkgs.OfType<InstalledPackage>().FirstOrDefault()?.InstallLocation ??
                                pkgs.OfType<string>().FirstOrDefault();
            }
            else if (navigationContext.Parameters.First().Value is InstalledPackage ip)
            {
                this.FilePath = ip.InstallLocation;
            }
            else if (navigationContext.Parameters.First().Value is string fp)
            {
                this.FilePath = fp;
            }
            else
            {
                this.FilePath = null;
            }
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        public string FilePath
        {
            get => this.filePath;
            set
            {
                if (!this.SetField(ref this.filePath, value))
                {
                    return;
                }
            }
        }
    }
}
