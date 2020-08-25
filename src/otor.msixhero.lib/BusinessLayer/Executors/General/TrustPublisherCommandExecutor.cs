using System;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Signing;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation.Enums;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Lib.BusinessLayer.State;
using Otor.MsixHero.Lib.Proxy.Signing.Dto;

namespace Otor.MsixHero.Lib.BusinessLayer.Executors.General
{
    public class TrustPublisherCommandExecutor : CommandExecutor
    {
        private readonly TrustDto command;
        private readonly ISelfElevationProxyProvider<ISigningManager> signingManagerFactory;

        public TrustPublisherCommandExecutor(
            TrustDto command,
            ISelfElevationProxyProvider<ISigningManager> signingManagerFactory) : base(command)
        {
            this.command = command;
            this.signingManagerFactory = signingManagerFactory;
        }

        public override async Task Execute(IInteractionService interactionService, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var manager = await this.signingManagerFactory.GetProxyFor(SelfElevationLevel.AsAdministrator, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            await manager.Trust(this.command.FilePath, cancellationToken).ConfigureAwait(false);
        }
    }
}