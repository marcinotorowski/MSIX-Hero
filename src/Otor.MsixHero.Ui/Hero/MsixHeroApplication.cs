using System;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Logging;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Ui.Hero.Executor;
using Otor.MsixHero.Ui.Hero.State;
using Prism.Events;
using LogManager = Otor.MsixHero.Infrastructure.Logging.LogManager;

namespace Otor.MsixHero.Ui.Hero
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

                this.ApplicationState.Packages.PackageFilter = configuration.List?.Filter?.PackageFilter ?? PackageFilter.Default;
                this.ApplicationState.Packages.ShowSidebar = configuration.List?.Sidebar?.Visible != false;
                this.ApplicationState.Packages.SortMode = configuration.List?.Sorting?.SortingMode ?? PackageSort.Name;
                this.ApplicationState.Packages.SortDescending = configuration.List?.Sorting?.Descending == true;
                this.ApplicationState.Packages.GroupMode = configuration.List?.Group?.GroupMode ?? PackageGroup.None;
            }
            catch (Exception e)
            {
                Logger.Warn(e, "Settings could not be restored upon exiting.");
            }
        }

    }
}