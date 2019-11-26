using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Commands.Signing;
using otor.msixhero.lib.BusinessLayer.Infrastructure;
using otor.msixhero.lib.BusinessLayer.Infrastructure.Implementation;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Managers;
using otor.msixhero.lib.Services;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    public class InstallCertificateReducer : SelfElevationReducer<ApplicationState>
    {
        private readonly InstallCertificate command;
        private readonly IBusyManager busyManager;

        public InstallCertificateReducer(InstallCertificate command, IApplicationStateManager<ApplicationState> stateManager, IBusyManager busyManager) : base(command, stateManager)
        {
            this.command = command;
            this.busyManager = busyManager;
        }

        public override async Task Reduce(IInteractionService interactionService, IAppxPackageManager packageManager, CancellationToken cancellationToken = default)
        {
            IBusyContext context = null;
            try
            {
                context = this.busyManager.Begin();
                await packageManager.InstallCertificate(this.command.FilePath, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                this.busyManager.End(context);
            }
        }
    }
}
