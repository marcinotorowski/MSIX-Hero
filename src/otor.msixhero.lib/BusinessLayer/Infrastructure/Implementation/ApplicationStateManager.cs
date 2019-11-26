using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Ipc;
using otor.msixhero.lib.Managers;
using otor.msixhero.lib.Services;
using Prism.Events;

namespace otor.msixhero.lib.BusinessLayer.Infrastructure.Implementation
{
    public class ApplicationStateManager : IApplicationStateManager<ApplicationState>
    {
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
            this.CurrentState.Configuration = await this.configurationService.GetConfiguration(CancellationToken.None).ConfigureAwait(false);
        }
    }
}