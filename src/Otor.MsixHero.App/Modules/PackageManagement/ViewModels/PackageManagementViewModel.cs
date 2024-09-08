// MSIX Hero
// Copyright (C) 2024 Marcin Otorowski
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

using System;
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Hero.Commands.Packages;
using Otor.MsixHero.App.Hero.Events.Base;
using Otor.MsixHero.App.Modules.PackageManagement.Commands;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.App.Mvvm.Progress;
using Otor.MsixHero.Elevation;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Events;
using Prism.Navigation;

namespace Otor.MsixHero.App.Modules.PackageManagement.ViewModels
{
    public class PackageManagementViewModel : NotifyPropertyChanged
    {
        private readonly IMsixHeroApplication _application;
        private readonly PrismServices _prismServices;

        public PackageManagementViewModel(
            IMsixHeroApplication application,
            IInteractionService interactionService,
            IUacElevation uacElevation,
            PrismServices prismServices,
            IBusyManager busyManager, 
            IConfigurationService configurationService)
        {
            this._application = application;
            this._prismServices = prismServices;
            this.CommandHandler = new PackagesManagementCommandHandler(
                application, 
                interactionService, 
                configurationService,
                prismServices, 
                uacElevation,
                busyManager);

            application.EventAggregator.GetEvent<UiExecutedEvent<SelectPackagesCommand>>().Subscribe(this.OnSelectPackages, ThreadOption.UIThread);
        }

        private void OnSelectPackages(UiExecutedPayload<SelectPackagesCommand> obj)
        {
            var parameters = new NavigationParameters
            {
                { "packages", this._application.ApplicationState.Packages.SelectedPackages }
            };

            switch (this._application.ApplicationState.Packages.SelectedPackages.Count)
            {
                case 0:
                    this._prismServices.RegionManager.Regions[PackageManagementRegionNames.Details].RequestNavigate(new Uri(NavigationPaths.PackageManagementPaths.ZeroSelection, UriKind.Relative), _ => { }, parameters);
                    break;
                case 1:
                    this._prismServices.RegionManager.Regions[PackageManagementRegionNames.Details].RequestNavigate(new Uri(NavigationPaths.PackageManagementPaths.SingleSelection, UriKind.Relative), _ => { }, parameters);
                    break;
                default:
                    this._prismServices.RegionManager.Regions[PackageManagementRegionNames.Details].NavigationService.RequestNavigate(new Uri(NavigationPaths.PackageManagementPaths.MultipleSelection, UriKind.Relative), _ => { }, parameters);
                    break;
            }
        }

        public PackagesManagementCommandHandler CommandHandler { get; }
    }
}
