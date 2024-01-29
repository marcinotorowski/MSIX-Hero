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
using Otor.MsixHero.App.Hero.Commands.EventViewer;
using Otor.MsixHero.App.Hero.Events.Base;
using Otor.MsixHero.App.Mvvm;
using Prism.Regions;

namespace Otor.MsixHero.App.Modules.EventViewer.ViewModels
{
    public class EventViewerViewModel : NotifyPropertyChanged
    {
        private readonly IMsixHeroApplication application;
        private readonly PrismServices prismServices;

        public EventViewerViewModel(
            IMsixHeroApplication application,
            PrismServices prismServices)
        {
            this.application = application;
            this.prismServices = prismServices;
            application.EventAggregator.GetEvent<UiExecutedEvent<SelectEventCommand>>().Subscribe(this.OnSelectLogCommand);
        }
        
        private void OnSelectLogCommand(UiExecutedPayload<SelectEventCommand> command)
        {
            var parameters = new NavigationParameters();

            if (command.Request.SelectedAppxEvent == null)
            {
                prismServices.RegionManager.Regions[EventViewerRegionNames.Details].RequestNavigate(new Uri(NavigationPaths.EventViewerPaths.NoDetails, UriKind.Relative), parameters);
            }
            else
            {
                parameters.Add("selection", this.application.ApplicationState.EventViewer.SelectedAppxEvent);
                prismServices.RegionManager.Regions[EventViewerRegionNames.Details].RequestNavigate(new Uri(NavigationPaths.EventViewerPaths.Details, UriKind.Relative), parameters);
            }
        }
    }
}

