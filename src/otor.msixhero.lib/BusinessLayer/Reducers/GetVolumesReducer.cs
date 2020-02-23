using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Appx;
using otor.msixhero.lib.BusinessLayer.Appx.VolumeManager;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Appx.Volume;
using otor.msixhero.lib.Domain.Commands.Volumes;
using otor.msixhero.lib.Domain.Events.Volumes;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Progress;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    internal class GetVolumesReducer : BaseReducer<List<AppxVolume>>
    {
        // ReSharper disable once NotAccessedField.Local
        private readonly GetVolumes command;
        private readonly IAppxVolumeManager volumeManager;
        private readonly IBusyManager busyManager;

        public GetVolumesReducer(GetVolumes command, IAppxVolumeManager volumeManager, IWritableApplicationStateManager stateManager, IBusyManager busyManager) : base(command, stateManager)
        {
            this.command = command;
            this.volumeManager = volumeManager;
            this.busyManager = busyManager;
        }

        public override async Task<List<AppxVolume>> GetReduced(IInteractionService interactionService, IAppxPackageManager packageManager, CancellationToken cancellationToken = default)
        {
            var context = this.busyManager.Begin();

            try
            {
                context.Report(new ProgressData(0, "Getting volumes..."));
                await Task.Delay(TimeSpan.FromMilliseconds(500)).ConfigureAwait(false);
                context.Report(new ProgressData(20, "Getting volumes..."));
                var defaultVolume = await this.volumeManager.GetDefault(cancellationToken, context).ConfigureAwait(false);

                context.Report(new ProgressData(80, "Getting volumes..."));
                var allVolumes = await this.volumeManager.GetAll(cancellationToken, context).ConfigureAwait(false);

                context.Report(new ProgressData(100, "Getting volumes..."));

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
            finally
            {
                this.busyManager.End(context);
            }
        }
    }
}