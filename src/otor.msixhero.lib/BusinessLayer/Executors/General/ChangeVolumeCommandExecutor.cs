using System;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Managers.Volumes;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Commands.Packages.Manager;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Progress;
using otor.msixhero.lib.Infrastructure.SelfElevation;

namespace otor.msixhero.lib.BusinessLayer.Executors.General
{
    internal class ChangeVolumeCommandExecutor : CommandExecutor
    {
        private readonly ChangeVolume changeVolumeCommand;
        private readonly ISelfElevationManagerFactory<IAppxVolumeManager> volumeManagerFactory;

        public ChangeVolumeCommandExecutor(ChangeVolume changeVolumeCommand, ISelfElevationManagerFactory<IAppxVolumeManager> volumeManagerFactory, IWritableApplicationStateManager state) : base(changeVolumeCommand, state)
        {
            this.changeVolumeCommand = changeVolumeCommand;
            this.volumeManagerFactory = volumeManagerFactory;
        }

        public override async Task Execute(IInteractionService interactionService, CancellationToken cancellationToken = default, IProgress<ProgressData> progressData = default)
        {
            var manager = await this.volumeManagerFactory.Get(SelfElevationLevel.AsAdministrator, cancellationToken).ConfigureAwait(false);
            await manager.MovePackageToVolume(this.changeVolumeCommand.VolumePackagePath, this.changeVolumeCommand.PackageFullName, cancellationToken, progressData).ConfigureAwait(false);
        }
    }
}
