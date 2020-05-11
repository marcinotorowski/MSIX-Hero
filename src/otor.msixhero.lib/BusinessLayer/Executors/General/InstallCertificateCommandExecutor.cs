using System;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Managers.Signing;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Commands.Packages.Signing;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Progress;
using otor.msixhero.lib.Infrastructure.SelfElevation;

namespace otor.msixhero.lib.BusinessLayer.Executors.General
{
    public class InstallCertificateCommandExecutor : CommandExecutor
    {
        private readonly InstallCertificate command;
        private readonly ISelfElevationManagerFactory<ISigningManager> signingManagerFactory;

        public InstallCertificateCommandExecutor(
            InstallCertificate command,
            ISelfElevationManagerFactory<ISigningManager> signingManagerFactory,
            IWritableApplicationStateManager stateManager) : base(command, stateManager)
        {
            this.command = command;
            this.signingManagerFactory = signingManagerFactory;
        }

        public override async Task Execute(IInteractionService interactionService, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var manager = await this.signingManagerFactory.Get(SelfElevationLevel.AsInvoker, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            await manager.InstallCertificate(this.command.FilePath, cancellationToken, progress).ConfigureAwait(false);
        }
    }
}
