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
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.Enums;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Common;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;
using Otor.MsixHero.Infrastructure.Localization;
using Prism.Commands;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Overview.Summaries
{
    public class SummaryApplicationsViewModel : NotifyPropertyChanged, ILoadPackage
    {
        public SummaryApplicationsViewModel(IPackageContentItemNavigation navigation)
        {
            this.Details = new DelegateCommand(() => navigation.SetCurrentItem(PackageContentViewType.Applications));
        }

        public ICommand Details { get; }
        
        public string SecondLine { get; private set; }

        public Task LoadPackage(AppxPackage model, string filePath, CancellationToken cancellationToken)
        {
            var promotedApplication = model.Applications.FirstOrDefault(ma => ma.Visible) ??
                                      model.Applications.FirstOrDefault(a => a.ExecutionAlias?.Any(ea => !string.IsNullOrEmpty(ea)) == true) ??
                                      model.Applications.FirstOrDefault();

            if (promotedApplication == null)
            {
                // no applications
                this.SecondLine = Resources.Localization.PackageExpert_Applications_None;
            }
            else
            {
                var otherApps = model.Applications.Count - 1;
                var promotedName = promotedApplication.DisplayName;
                if (promotedApplication.EntryPoint == "Windows.FullTrustApplication")
                {
                    promotedName += " [" + Path.GetFileName(promotedApplication.Psf?.Executable ?? promotedApplication.Executable) + "]";
                }

                switch (otherApps)
                {
                    case 0:
                        this.SecondLine = promotedName;
                        break;
                    case 1:
                        this.SecondLine = string.Format(Resources.Localization.PackageExpert_Applications_OneExtra, promotedName);
                        break;
                    default:
                        this.SecondLine = string.Format(Resources.Localization.PackageExpert_Applications_XExtra, promotedName, 1);
                        break;
                }
            }

            this.AppCount = model.Applications.Count;
            this.Apps = new ObservableCollection<ApplicationVisualSummaryViewModel>(model.Applications.Where(a => a.Visible).Select(app => new ApplicationVisualSummaryViewModel(app, model)));

            this.OnPropertyChanged(null);
            return Task.CompletedTask;
        }

        public int AppCount { get; private set; }

        public bool HasApplications => this.AppCount > 0;

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
