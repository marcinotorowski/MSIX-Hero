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
    public class TrustPublisherCommandExecutor : CommandExecutor
    {
        private readonly TrustPublisher command;
        private readonly ISelfElevationManagerFactory<ISigningManager> signingManagerFactory;

        public TrustPublisherCommandExecutor(
            TrustPublisher command,
            ISelfElevationManagerFactory<ISigningManager> signingManagerFactory,
            IWritableApplicationStateManager stateManager) : base(command, stateManager)
        {
            this.command = command;
            this.signingManagerFactory = signingManagerFactory;
        }

        public override async Task Execute(IInteractionService interactionService, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var manager = await this.signingManagerFactory.Get(SelfElevationLevel.AsAdministrator, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            await manager.Trust(this.command.FilePath, cancellationToken).ConfigureAwait(false);
        }
    }
}