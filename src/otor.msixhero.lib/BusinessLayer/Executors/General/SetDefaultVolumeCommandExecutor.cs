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
    internal class SetDefaultVolumeCommandExecutor : CommandExecutor
    {
        private readonly SetDefaultDto command;
        private readonly ISelfElevationProxyProvider<IAppxVolumeManager> volumeManager;

        public SetDefaultVolumeCommandExecutor(SetDefaultDto command, ISelfElevationProxyProvider<IAppxVolumeManager> volumeManager) : base(command)
        {
            this.command = command;
            this.volumeManager = volumeManager;
        }

        public override async Task Execute(IInteractionService interactionService, CancellationToken cancellationToken = default, IProgress<ProgressData> progressData = default)
        {
            var manager = await this.volumeManager.GetProxyFor(SelfElevationLevel.AsAdministrator, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            await manager.SetDefault(command.DrivePath, cancellationToken).ConfigureAwait(false);
        }
    }
}
