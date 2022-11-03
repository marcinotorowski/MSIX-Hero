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

using System;
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Hero.Commands.Containers;
using Otor.MsixHero.App.Hero.Events.Base;
using Otor.MsixHero.App.Modules.Containers.Commands;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.App.Mvvm.Progress;
using Otor.MsixHero.Elevation;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Regions;

namespace Otor.MsixHero.App.Modules.Containers.ViewModels
{
    public class ContainersViewModel : NotifyPropertyChanged
    {
        private readonly IMsixHeroApplication _application;
        private readonly PrismServices _prismServices;

        public ContainersViewModel(
            IUacElevation uac,
            IMsixHeroApplication application,
            IInteractionService interactionService,
            IBusyManager busyManager,
            PrismServices prismServices)
        {
            this._application = application;
            this._prismServices = prismServices;
            this._application.EventAggregator.GetEvent<UiExecutedEvent<SelectSharedPackageContainerCommand>>().Subscribe(this.OnSelect);
            this.CommandHandler = new ContainersCommandHandler(uac, application, interactionService, busyManager, prismServices);
        }
        public ContainersCommandHandler CommandHandler { get; }

        private void OnSelect(UiExecutedPayload<SelectSharedPackageContainerCommand> command)
        {
            var parameters = new NavigationParameters();

            if (command.Request.SelectedContainer == null)
            {
                _prismServices.RegionManager.Regions[ContainersRegionNames.Details].RequestNavigate(new Uri(NavigationPaths.ContainersPaths.NoDetails, UriKind.Relative), parameters);
            }
            else
            {
                parameters.Add("selection", this._application.ApplicationState.Containers.SelectedContainer);
                _prismServices.RegionManager.Regions[ContainersRegionNames.Details].RequestNavigate(new Uri(NavigationPaths.ContainersPaths.Details, UriKind.Relative), parameters);
            }
        }
    }
}