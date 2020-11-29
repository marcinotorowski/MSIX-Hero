using System;
using Otor.MsixHero.App.Hero.Executor;
using Otor.MsixHero.App.Hero.State;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Logging;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Events;
using LogManager = Otor.MsixHero.Infrastructure.Logging.LogManager;

namespace Otor.MsixHero.App.Hero
{
    public class MsixHeroApplication : IMsixHeroApplication
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(MsixHeroApplication));

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
            }
            catch (Exception e)
            {
                Logger.Warn(e, "Settings could not be restored upon exiting.");
            }
        }

    }
}