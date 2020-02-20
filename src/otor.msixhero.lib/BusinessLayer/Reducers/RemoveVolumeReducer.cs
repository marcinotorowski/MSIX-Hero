using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Appx;
using otor.msixhero.lib.BusinessLayer.Appx.VolumeManager;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Commands.Volumes;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Interop;
using otor.msixhero.lib.Infrastructure.Ipc;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    internal class RemoveVolumeReducer : SelfElevationReducer
    {
        private readonly RemoveVolume command;
        private readonly IAppxVolumeManager volumeManager;
        private readonly IProcessManager processManager;

        public RemoveVolumeReducer(RemoveVolume command, IAppxVolumeManager volumeManager, IProcessManager processManager, IWritableApplicationStateManager stateManager) : base(command, stateManager)
        {
            this.command = command;
            this.volumeManager = volumeManager;
            this.processManager = processManager;
        }

        public override Task Reduce(IInteractionService interactionService, IAppxPackageManager packageManager, CancellationToken cancellationToken = default)
        {
            if (this.StateManager.CurrentState.IsElevated)
            {
                return this.volumeManager.Delete(command.Name, cancellationToken);
            }

            var client = new Client(this.processManager);
            return client.Execute(this.command, cancellationToken);
        }
    }
}