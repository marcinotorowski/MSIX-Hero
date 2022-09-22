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

using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Common;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel
{
    public class HeaderViewModel : NotifyPropertyChanged, ILoadPackage
    {
        private bool _isLoading;
        
        public bool IsLoading
        {
            get => this._isLoading;
            private set => this.SetField(ref this._isLoading, value);
        }

        public Task LoadPackage(AppxPackage model, PackageEntry installationEntry, string filePath, CancellationToken cancellationToken)
        {
            try
            {
                this.IsLoading = true;
                this.DisplayName = model.DisplayName;
                this.PublisherDisplayName = model.PublisherDisplayName;
                this.Publisher = model.Publisher;
                this.Version = model.Version;
                this.Logo = model.Logo;
                this.TileColor = null;

                if (model.Applications != null)
                {
                    foreach (var item in model.Applications)
                    {
                        if (string.IsNullOrEmpty(TileColor))
                        {
                            this.TileColor = item.BackgroundColor;
                            break;
                        }
                    }
                }
                
                this.OnPropertyChanged(null);
                return Task.CompletedTask;
            }
            finally
            {
                this.IsLoading = false;
            }
        }

        public string PublisherDisplayName { get; private set; }

        public string TileColor { get; private set; }

        public string DisplayName { get; private set; }

        public byte[] Logo { get; private set; }

        public string Version { get; private set; }

        public string Publisher { get; private set; }
    }
}
