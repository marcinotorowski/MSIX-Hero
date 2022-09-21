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

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.Enums;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Common;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;
using Prism.Commands;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Overview.Summaries
{
    public class SummaryDependenciesViewModel : NotifyPropertyChanged, ILoadPackage
    {
        public SummaryDependenciesViewModel(IPackageContentItemNavigation navigation)
        {
            this.Details = new DelegateCommand(() => navigation.SetCurrentItem(PackageContentViewType.Dependencies));
        }

        public ICommand Details { get; }

        public bool HasAny { get; private set; }

        public int Count { get; private set; }

        public string Summary { get; private set; }

        public Task LoadPackage(AppxPackage model, PackageEntry installEntry, string filePath, CancellationToken cancellationToken)
        {
            this.Count = model.OperatingSystemDependencies.Count + model.PackageDependencies.Count;
            this.HasAny = this.Count > 0;

            if (model.OperatingSystemDependencies.Any())
            {
                if (this.Count == 1)
                {
                    this.Summary = FormatSystemDependency(model.OperatingSystemDependencies[0].Minimum);
                }
                else
                {
                    this.Summary = string.Format(Resources.Localization.PackageExpert_Dependencies_Summary_XOther, FormatSystemDependency(model.OperatingSystemDependencies[0].Minimum), this.Count - 1);
                }
            }
            else if (model.PackageDependencies.Any())
            {
                if (this.Count - 1 == 0)
                {
                    this.Summary = model.PackageDependencies[0].Name;
                }
                else
                {
                    this.Summary = string.Format(Resources.Localization.PackageExpert_Dependencies_Summary_XOther, model.PackageDependencies[0].Name, this.Count - 1);
                }
            }
            else
            {
                this.Summary = Resources.Localization.PackageExpert_Dependencies_Summary_None;
            }
            
            this.OnPropertyChanged(null);

            return Task.CompletedTask;
        }

        private static string FormatSystemDependency(AppxTargetOperatingSystem os)
        {
            if (string.IsNullOrEmpty(os.MarketingCodename))
            {
                return os.Name;
            }

            return $"{os.Name} ({os.MarketingCodename})";
        }
    }
}
