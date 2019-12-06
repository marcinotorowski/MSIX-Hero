using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Appx;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Commands.Signing;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Progress;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    public class InstallCertificateReducer : SelfElevationReducer
    {
        private readonly InstallCertificate command;
        private readonly IBusyManager busyManager;

        public InstallCertificateReducer(InstallCertificate command, IWritableApplicationStateManager stateManager, IBusyManager busyManager) : base(command, stateManager)
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
