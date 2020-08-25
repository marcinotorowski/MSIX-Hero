using System;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Volumes;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation.Enums;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Lib.Proxy.Volumes.Dto;

namespace Otor.MsixHero.Lib.BusinessLayer.Executors.General
{
    internal class DismountVolumeCommandExecutor : CommandExecutor
    {
        private readonly DismountDto command;
        private readonly ISelfElevationProxyProvider<IAppxVolumeManager> volumeManager;

        public DismountVolumeCommandExecutor(DismountDto command, ISelfElevationProxyProvider<IAppxVolumeManager> volumeManager) : base(command)
        {
            this.command = command;
            this.volumeManager = volumeManager;
        }

        public override async Task Execute(IInteractionService interactionService, CancellationToken cancellationToken = default, IProgress<ProgressData> progressData = default)
        {
            var manager = await this.volumeManager.GetProxyFor(SelfElevationLevel.AsAdministrator, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            await manager.Dismount(command.Name, cancellationToken).ConfigureAwait(false);
        }
    }
}