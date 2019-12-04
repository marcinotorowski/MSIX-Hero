using System;
using otor.msixhero.lib.BusinessLayer.Appx;
using otor.msixhero.lib.Domain.State;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Commanding;
using otor.msixhero.lib.Infrastructure.Configuration;
using otor.msixhero.lib.Infrastructure.Logging;
using Prism.Events;

namespace otor.msixhero.lib.BusinessLayer.State
{
    public class ApplicationStateManager : IDisposable, IApplicationStateManager<ApplicationState>
    {
        private static readonly ILog Logger = LogManager.GetLogger();

        private readonly IConfigurationService configurationService;

        public ApplicationStateManager(
            IEventAggregator eventAggregator,
            IAppxPackageManagerFactory packageManagerFactory,
            IBusyManager busyManager,
            IInteractionService interactionService,
            IConfigurationService configurationService)
        {
            this.configurationService = configurationService;
            this.CurrentState = new ApplicationState();
            this.CommandExecutor = new CommandExecutor(this, packageManagerFactory, interactionService, busyManager);
            this.EventAggregator = eventAggregator;
            this.RestoreSettings();
        }

        public ApplicationState CurrentState { get; }

        IApplicationState IApplicationStateManager.CurrentState => this.CurrentState;

        public ICommandExecutor CommandExecutor { get; }

        public IEventAggregator EventAggregator { get; }

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