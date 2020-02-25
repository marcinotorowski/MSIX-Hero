using System;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Appx.VolumeManager;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Appx.Volume;
using otor.msixhero.lib.Domain.Commands.Volumes;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Ipc;
using otor.msixhero.lib.Infrastructure.Progress;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    internal class AddVolumeReducer : SelfElevationReducer<AppxVolume>
    {
        private readonly AddVolume command;
        private readonly IAppxVolumeManager volumeManager;

        public AddVolumeReducer(AddVolume command, IAppxVolumeManager volumeManager, IElevatedClient elevatedClient, IWritableApplicationStateManager stateManager) : base(command, elevatedClient, stateManager)
        {
            this.command = command;
            this.volumeManager = volumeManager;
        }

        protected override Task<AppxVolume> GetReducedAsCurrentUser(IInteractionService interactionService, CancellationToken cancellationToken = default, IProgress<ProgressData> progressReporter = default)
        {
            return this.volumeManager.Add(command.DrivePath, cancellationToken);
        }
    }
}
