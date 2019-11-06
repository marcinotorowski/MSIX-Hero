using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Commands;
using otor.msixhero.lib.BusinessLayer.Commands.Signing;
using otor.msixhero.lib.BusinessLayer.Infrastructure;
using otor.msixhero.lib.BusinessLayer.Infrastructure.Implementation;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Ipc;
using otor.msixhero.lib.Managers;
using otor.msixhero.lib.Services;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    public class InstallCertificateReducer : SelfElevationReducer<ApplicationState, bool>
    {
        private readonly InstallCertificate command;
        private readonly IAppxSigningManager signingManager;
        private readonly IBusyManager busyManager;

        public InstallCertificateReducer(InstallCertificate command, IApplicationStateManager<ApplicationState> stateManager, IClientCommandRemoting clientCommandRemoting, IAppxSigningManager signingManager, IBusyManager busyManager) : base(command, stateManager, clientCommandRemoting)
        {
            this.command = command;
            this.signingManager = signingManager;
            this.busyManager = busyManager;
        }

        public override async Task<bool> GetReduced(IInteractionService interactionService, CancellationToken cancellationToken = default)
        {
            IBusyContext context = null;
            try
            {
                context = this.busyManager.Begin();
                if (this.command.RequiresElevation)
                {
                    await this.clientCommandRemoting.GetClientInstance().Execute(this.command, cancellationToken, context).ConfigureAwait(false);
                }
                else
                {
                    await this.signingManager.InstallCertificate(this.command.FilePath, cancellationToken, context).ConfigureAwait(false);
                }
            }
            finally
            {
                this.busyManager.End(context);
            }

            return true;
        }
    }
}
