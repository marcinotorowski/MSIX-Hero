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

using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Common;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities.Build;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Overview.Summaries
{
    public class SummaryPackagingInformationViewModel : NotifyPropertyChanged, ILoadPackage
    {
        public Task LoadPackage(AppxPackage model, PackageEntry installEntry, string filePath, CancellationToken cancellationToken)
        {
            this.BuildInfo = model.BuildInfo;
            this.WindowsVersion = model.BuildInfo?.OperatingSystem?.TechnicalVersion;

            if (this.BuildInfo?.Components != null)
            {
                this.Components = new ObservableCollection<PackagingInformationComponentViewModel>(this.BuildInfo.Components.Select(kv => new PackagingInformationComponentViewModel(kv.Key, kv.Value)));
            }
            else
            {
                this.Components = null;
            }

            this.OnPropertyChanged(null);
            return Task.CompletedTask;
        }

        public bool HasBuildInfo => this.BuildInfo != null;

        public bool HasComponents => this.Components?.Any() == true;

        public BuildInfo BuildInfo{ get; private set; }

        public string WindowsVersion { get; private set; }

        public ObservableCollection<PackagingInformationComponentViewModel> Components { get; private set; }

        public class PackagingInformationComponentViewModel
        {
            public PackagingInformationComponentViewModel(string name, string version)
            {
                Name = name;
                Version = version;
            }

            public string Name { get; }

            public string Version { get; }
        }
    }
}
