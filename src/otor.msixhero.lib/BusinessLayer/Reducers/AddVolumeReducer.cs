using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Appx;
using otor.msixhero.lib.BusinessLayer.Appx.VolumeManager;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Appx.Volume;
using otor.msixhero.lib.Domain.Commands.Volumes;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Interop;
using otor.msixhero.lib.Infrastructure.Ipc;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    internal class AddVolumeReducer : SelfElevationReducer<AppxVolume>
    {
        private readonly AddVolume command;
        private readonly IAppxVolumeManager volumeManager;
        private readonly IProcessManager processManager;

        public AddVolumeReducer(AddVolume command, IAppxVolumeManager volumeManager, IProcessManager processManager, IWritableApplicationStateManager stateManager) : base(command, stateManager)
        {
            this.command = command;
            this.volumeManager = volumeManager;
            this.processManager = processManager;
        }

        public override Task<AppxVolume> GetReduced(IInteractionService interactionService, IAppxPackageManager packageManager, CancellationToken cancellationToken = default)
        {
            if (this.StateManager.CurrentState.IsElevated)
            {
                return this.volumeManager.Add(command.DrivePath, cancellationToken);
            }

            var client = new Client(this.processManager);
            return client.GetExecuted(this.command, cancellationToken);
        }
    }
}
