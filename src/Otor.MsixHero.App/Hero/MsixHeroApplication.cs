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
using Otor.MsixHero.App.Hero.Executor;
using Otor.MsixHero.App.Hero.State;
using Otor.MsixHero.Infrastructure.Configuration;
using Dapplo.Log;
using Otor.MsixHero.Appx.Diagnostic.Events;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Events;

namespace Otor.MsixHero.App.Hero
{
    public class MsixHeroApplication : IMsixHeroApplication
    {
        private static readonly LogSource Logger = new();

        public MsixHeroApplication(
            IMsixHeroCommandExecutor commandExecutor, 
            IConfigurationService configurationService,
            IEventAggregator eventAggregator)
        {
            this.ApplicationState = new MsixHeroState();
            this.CommandExecutor = commandExecutor;
            this.EventAggregator = eventAggregator;
            this.ConfigurationService = configurationService;

            this.RestoreSettings();
            this.CommandExecutor.ApplicationState = this.ApplicationState;
        }

        public IMsixHeroCommandExecutor CommandExecutor { get; }

        public IEventAggregator EventAggregator { get; }

        public MsixHeroState ApplicationState { get; }

        public IConfigurationService ConfigurationService { get; }

        private void RestoreSettings()
        {
            try
            {
                var configuration = this.ConfigurationService.GetCurrentConfiguration();

                if (configuration == null)
                {
                    return;
                }

                this.ApplicationState.Packages.Filter = configuration.Packages?.Filter?.Filter ?? PackageFilter.Default;
                this.ApplicationState.Packages.ShowSidebar = configuration.Packages?.Sidebar?.Visible != false;
                this.ApplicationState.Packages.SortMode = configuration.Packages?.Sorting?.SortingMode ?? PackageSort.Name;
                this.ApplicationState.Packages.SortDescending = configuration.Packages?.Sorting?.Descending == true;
                this.ApplicationState.Packages.GroupMode = configuration.Packages?.Group?.GroupMode ?? PackageGroup.None;

                this.ApplicationState.EventViewer.SortMode = configuration.Events?.Sorting?.SortingMode ?? EventSort.Date;
                this.ApplicationState.EventViewer.SortDescending = configuration.Events?.Sorting?.Descending == true;
                this.ApplicationState.EventViewer.Filter = configuration.Events?.Filter?.Filter ?? EventFilter.Warning | EventFilter.Error;
                this.ApplicationState.EventViewer.Criteria = configuration.Events?.TimeSpan == default ? new EventCriteria() : new EventCriteria(configuration.Events.TimeSpan);
            }
            catch (Exception e)
            {
                Logger.Warn().WriteLine("Settings could not be restored upon exiting.");
                Logger.Warn().WriteLine(e);
            }
        }
    }
}