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
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Common;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel
{
    public class HeaderViewModel : NotifyPropertyChanged, ILoadPackage
    {
        public HeaderViewModel(IPackageContentItemNavigation _)
        {
        }

        public Task LoadPackage(AppxPackage model, string filePath)
        {
            DisplayName = model.DisplayName;
            PublisherDisplayName = model.PublisherDisplayName;
            Publisher = model.Publisher;
            Version = model.Version;
            Logo = model.Logo;

            TileColor = null;

            if (model.Applications != null)
            {
                foreach (var item in model.Applications)
                {
                    if (string.IsNullOrEmpty(TileColor))
                    {
                        TileColor = item.BackgroundColor;
                        break;
                    }
                }
            }

            if (string.IsNullOrEmpty(TileColor))
            {
                TileColor = "#666666";
            }

            OnPropertyChanged(null);
            return Task.CompletedTask;
        }

        public string PublisherDisplayName { get; private set; }

        public string TileColor { get; private set; }

        public string DisplayName { get; private set; }

        public byte[] Logo { get; private set; }

        public string Version { get; private set; }

        public string Publisher { get; private set; }
    }
}
