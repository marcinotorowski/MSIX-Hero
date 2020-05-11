using System;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Managers.Volumes;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Commands.Volumes;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Progress;
using otor.msixhero.lib.Infrastructure.SelfElevation;

namespace otor.msixhero.lib.BusinessLayer.Executors.General
{
    internal class MountVolumeCommandExecutor : CommandExecutor
    {
        private readonly MountVolume command;
        private readonly ISelfElevationManagerFactory<IAppxVolumeManager> volumeManager;

        public MountVolumeCommandExecutor(MountVolume command, ISelfElevationManagerFactory<IAppxVolumeManager> volumeManager, IWritableApplicationStateManager stateManager) : base(command, stateManager)
        {
            this.command = command;
            this.volumeManager = volumeManager;
        }

        public override async Task Execute(IInteractionService interactionService, CancellationToken cancellationToken = default,
            IProgress<ProgressData> progressData = default)
        {
            var manager = await this.volumeManager.Get(SelfElevationLevel.AsAdministrator, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            await manager.Mount(command.Name, cancellationToken).ConfigureAwait(false);
        }
    }
}
