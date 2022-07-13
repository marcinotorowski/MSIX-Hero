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
using System.Threading.Tasks;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Common;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Overview.Summaries
{
    public class SummaryApplicationsViewModel : NotifyPropertyChanged, ILoadPackage
    {
        public SummaryApplicationsViewModel(IPackageContentItemNavigation navigation)
        {
        }

        public string SecondLine { get; private set; } = "Work in progress";

        public Task LoadPackage(AppxPackage model, string filePath)
        {
            var apps = new ObservableCollection<ApplicationVisualSummaryViewModel>();

            foreach (var app in model.Applications.Where(a => a.Visible))
            {
                apps.Add(new ApplicationVisualSummaryViewModel(app, model));
            }

            this.Apps = apps;
            this.OnPropertyChanged(null);
            return Task.CompletedTask;
        }

        public ObservableCollection<ApplicationVisualSummaryViewModel> Apps { get; private set; }

        public class ApplicationVisualSummaryViewModel
        {
            public ApplicationVisualSummaryViewModel(AppxApplication app, AppxPackage parentPackage)
            {
                this.Name = app.DisplayName;
                this.Logo = app.Logo ?? parentPackage.Logo;
                this.TileColor = app.BackgroundColor ?? "Transparent";
            }

            public string Name { get; }

            public byte[] Logo { get; }

            public string TileColor { get; }
        }
    }
}
