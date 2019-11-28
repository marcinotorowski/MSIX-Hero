using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Appx;
using otor.msixhero.lib.BusinessLayer.Appx.Signing;
using otor.msixhero.lib.BusinessLayer.Commanding;
using otor.msixhero.lib.Domain.State;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Commanding;
using otor.msixhero.lib.Infrastructure.Configuration;
using otor.msixhero.lib.Infrastructure.Ipc;
using otor.msixhero.lib.Infrastructure.Logging;
using Prism.Events;

namespace otor.msixhero.lib.BusinessLayer.State
{
    public class ApplicationStateManager : IApplicationStateManager<ApplicationState>
    {
        private static readonly ILog Logger = LogManager.GetLogger();

        private readonly IConfigurationService configurationService;

        public ApplicationStateManager(
            IEventAggregator eventAggregator,
            IAppxPackageManagerFactory packageManagerFactory,
            IAppxSigningManager signingManager,
            IBusyManager busyManager,
            IInteractionService interactionService,
            IClientCommandRemoting clientCommandRemoting,
            IConfigurationService configurationService)
        {
            this.configurationService = configurationService;
            this.CurrentState = new ApplicationState();
            this.CommandExecutor = new CommandExecutor(this, packageManagerFactory, interactionService, busyManager);
            this.EventAggregator = eventAggregator;
        }

        public ApplicationState CurrentState { get; }

        IApplicationState IApplicationStateManager.CurrentState => this.CurrentState;

        public ICommandExecutor CommandExecutor { get; }

        public IEventAggregator EventAggregator { get; }

        public async Task Initialize()
        {
            Logger.Debug("Initialize state manager configuration...");
            this.CurrentState.Configuration = await this.configurationService.GetConfiguration(CancellationToken.None).ConfigureAwait(false);
        }
    }
}