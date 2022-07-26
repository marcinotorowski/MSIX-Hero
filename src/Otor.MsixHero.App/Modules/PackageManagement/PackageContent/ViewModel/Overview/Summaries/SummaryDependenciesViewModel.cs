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

        public Task LoadPackage(AppxPackage model, string filePath, CancellationToken cancellationToken)
        {
            this.Count = model.OperatingSystemDependencies.Count + model.PackageDependencies.Count;
            this.HasAny = this.Count > 0;

            if (model.OperatingSystemDependencies.Any())
            {
                if (this.Count - 1 == 0)
                {
                    this.Summary = model.OperatingSystemDependencies[0].Minimum.Name;
                }
                else
                {
                    this.Summary = $"{model.OperatingSystemDependencies[0].Minimum.Name} and {this.Count - 1} other dependencies";
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
                    this.Summary = $"{model.PackageDependencies[0].Name} and {this.Count - 1} other dependencies";
                }
            }
            else
            {
                this.Summary = "This package does not have dependencies.";
            }
            
            this.OnPropertyChanged(null);

            return Task.CompletedTask;
        }
        
        public bool HasAny { get; private set; }
        
        public int Count { get; private set; }
        
        public string Summary { get; private set; }
    }
}
