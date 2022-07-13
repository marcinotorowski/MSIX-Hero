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
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Common;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;
using Prism.Commands;
using Prism.Services.Dialogs;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Overview.Summaries
{
    public class SummaryPackageNameViewModel : NotifyPropertyChanged, ILoadPackage
    {
        public SummaryPackageNameViewModel(IPackageContentItemNavigation navigation, PrismServices prismServices)
        {
            this.OpenPackageNameCalculator = new DelegateCommand(() =>
            {
                IDialogParameters parameters = new DialogParameters();
                parameters.Add("name", this.Name);
                parameters.Add("version", this.Version);
                parameters.Add("familyName", this.FamilyName);
                parameters.Add("publisher", this.Publisher);
                parameters.Add("architecture", this.Architecture);
                parameters.Add("resourceId", this.ResourceId);

                prismServices.ModuleManager.LoadModule(ModuleNames.Dialogs.Packaging);
                prismServices.DialogService.ShowDialog(NavigationPaths.DialogPaths.PackagingNames, parameters, _ => { });
            });
        }

        public ICommand OpenPackageNameCalculator { get; }

        public Task LoadPackage(AppxPackage model, string filePath)
        {
            this.DisplayName = model.DisplayName;
            this.Description = model.Description;
            this.Publisher = model.Publisher;
            this.ResourceId = model.ResourceId;
            this.FamilyName = model.FamilyName;
            this.Architecture = model.ProcessorArchitecture.ToString();
            this.PackageFullName = model.FullName;
            this.PublisherDisplayName = model.PublisherDisplayName;
            this.Version = model.Version;
            this.Name = model.Name;
            
            this.OnPropertyChanged(null);

            return Task.CompletedTask;
        }
        
        public string PackageFullName { get; private set; }

        public string Description { get; private set; }

        public string PublisherDisplayName { get; private set; }

        public string Publisher { get; private set; }
        
        public string DisplayName { get; private set; }
        
        public string FamilyName { get; private set; }

        public string Architecture { get; private set; }

        public string ResourceId { get; private set; }

        public string Version { get; private set; }

        public string Name { get; private set; }
    }
}
