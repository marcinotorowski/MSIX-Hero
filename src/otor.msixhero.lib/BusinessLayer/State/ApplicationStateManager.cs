using System;
using otor.msixhero.lib.Domain.State;
using otor.msixhero.lib.Infrastructure.Commanding;
using otor.msixhero.lib.Infrastructure.Configuration;
using otor.msixhero.lib.Infrastructure.Logging;
using Prism.Events;

namespace otor.msixhero.lib.BusinessLayer.State
{
    public class ApplicationStateManager : IDisposable, IWritableApplicationStateManager
    {
        private static readonly ILog Logger = LogManager.GetLogger();

        private readonly IConfigurationService configurationService;

        public ApplicationStateManager(
            IEventAggregator eventAggregator,
            ICommandBus commandExecutor,
            IConfigurationService configurationService)
        {
            this.configurationService = configurationService;
            this.CurrentState = new ApplicationState();
            this.CommandExecutor = commandExecutor;
            this.EventAggregator = eventAggregator;
            this.RestoreSettings();

            commandExecutor.SetStateManager(this);
        }

        public ApplicationState CurrentState { get; }

        IApplicationState IApplicationStateManager.CurrentState => this.CurrentState;

        public ICommandBus CommandExecutor { get; }

        public IEventAggregator EventAggregator { get; }

        IWritableApplicationState IWritableApplicationStateManager.CurrentState => this.CurrentState;

        private void RestoreSettings()
        {
            try
            {
                var configuration = configurationService.GetCurrentConfiguration();
                this.CurrentState.Packages.ShowSidebar = configuration.List.Sidebar.Visible;
                this.CurrentState.Packages.Filter = configuration.List.Filter.ShowApps;
            }
            catch (Exception e)
            {
                Logger.Warn(e, "Settings could not be restored upon exiting.");
            }
        }

        void IDisposable.Dispose()
        {
            try
            {
                var config = this.configurationService.GetCurrentConfiguration();
                config.List.Sidebar.Visible = this.CurrentState.Packages.ShowSidebar;
                config.List.Filter.ShowApps = this.CurrentState.Packages.Filter;
                this.configurationService.SetCurrentConfiguration(config);
            }
            catch (Exception e)
            {
                Logger.Warn(e, "Settings could not be save upon exiting.");
            }
        }
    }
}