using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Managers.Volumes;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Commands.Packages.Grid;
using otor.msixhero.lib.Domain.Commands.Volumes;
using otor.msixhero.lib.Domain.Events.PackageList;
using otor.msixhero.lib.Domain.Events.Volumes;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Progress;
using otor.msixhero.lib.Infrastructure.SelfElevation;
using CollectionChangeType = otor.msixhero.lib.Domain.Events.Volumes.CollectionChangeType;

namespace otor.msixhero.lib.BusinessLayer.Executors.Client
{
    internal class RemoveVolumeClientExecutor : CommandExecutor
    {
        private readonly RemoveVolume command;
        private readonly ISelfElevationManagerFactory<IAppxVolumeManager> volumeManagerFactory;

        public RemoveVolumeClientExecutor(RemoveVolume command, ISelfElevationManagerFactory<IAppxVolumeManager> volumeManagerFactory, IWritableApplicationStateManager stateManager) : base(command, stateManager)
        {
            this.command = command;
            this.volumeManagerFactory = volumeManagerFactory;
        }

        public override async Task Execute(IInteractionService interactionService, CancellationToken cancellationToken = default, IProgress<ProgressData> progressData = default)
        {
            var volumeManager = await this.volumeManagerFactory.Get(SelfElevationLevel.AsAdministrator, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();

            var selection = this.StateManager.CurrentState.Volumes.SelectedItems.FirstOrDefault(item => string.Equals(item.Name, command.Name, StringComparison.OrdinalIgnoreCase));

            await volumeManager.Delete(command.Name, cancellationToken, progressData).ConfigureAwait(false);

            if (selection != null)
            {
                await this.StateManager.CommandExecutor.ExecuteAsync(SelectVolumes.CreateSubtraction(selection), cancellationToken).ConfigureAwait(false);
            }

            var eventData = new VolumesCollectionChangedPayLoad(CollectionChangeType.Simple);
            this.StateManager.EventAggregator.GetEvent<VolumesCollectionChanged>().Publish(eventData);
        }
    }
}