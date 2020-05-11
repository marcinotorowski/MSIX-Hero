using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Managers.Volumes;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Appx.Volume;
using otor.msixhero.lib.Domain.Commands.Volumes;
using otor.msixhero.lib.Domain.Events.Volumes;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Progress;
using otor.msixhero.lib.Infrastructure.SelfElevation;

namespace otor.msixhero.lib.BusinessLayer.Executors.Client
{
    internal class GetVolumesClientExecutor : CommandWithOutputExecutor<List<AppxVolume>>
    {
        // ReSharper disable once NotAccessedField.Local
        private readonly GetVolumes command;
        private readonly ISelfElevationManagerFactory<IAppxVolumeManager> volumeManager;

        public GetVolumesClientExecutor(GetVolumes command, ISelfElevationManagerFactory<IAppxVolumeManager> volumeManager, IWritableApplicationStateManager stateManager) : base(command, stateManager)
        {
            this.command = command;
            this.volumeManager = volumeManager;
        }

        public override async Task<List<AppxVolume>> ExecuteAndReturn(IInteractionService interactionService, CancellationToken cancellationToken = default, IProgress<ProgressData> progressReporter = default)
        {
            var manager = await this.volumeManager.Get(SelfElevationLevel.AsInvoker, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();

            progressReporter?.Report(new ProgressData(0, "Getting volumes..."));
            await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken).ConfigureAwait(false);
            progressReporter?.Report(new ProgressData(20, "Getting volumes..."));
            var defaultVolume = await manager.GetDefault(cancellationToken).ConfigureAwait(false);

            progressReporter?.Report(new ProgressData(80, "Getting volumes..."));
            var allVolumes = await manager.GetAll(cancellationToken).ConfigureAwait(false);
                
            progressReporter?.Report(new ProgressData(100, "Getting volumes..."));
            if (defaultVolume != null)
            {
                defaultVolume = allVolumes.FirstOrDefault(v => string.Equals(v.PackageStorePath, defaultVolume.PackageStorePath, StringComparison.OrdinalIgnoreCase));
                if (defaultVolume != null)
                {
                    defaultVolume.IsDefault = true;
                }
            }

            var state = this.StateManager.CurrentState;
            var selectedPackageNames = new HashSet<string>(state.Volumes.SelectedItems.Select(item => item.PackageStorePath));

            state.Volumes.SelectedItems.Clear();
            state.Volumes.VisibleItems.Clear();
            state.Volumes.HiddenItems.Clear();
            state.Volumes.VisibleItems.AddRange(allVolumes);

            this.StateManager.EventAggregator.GetEvent<VolumesCollectionChanged>().Publish(new VolumesCollectionChangedPayLoad(CollectionChangeType.Reset));
            await this.StateManager.CommandExecutor.ExecuteAsync(new SetVolumeFilter(state.Volumes.SearchKey), cancellationToken).ConfigureAwait(false);
            await this.StateManager.CommandExecutor.ExecuteAsync(new SelectVolumes(state.Volumes.VisibleItems.Where(item => selectedPackageNames.Contains(item.PackageStorePath)).ToList()), cancellationToken).ConfigureAwait(false);

            return allVolumes;
        }
    }
}