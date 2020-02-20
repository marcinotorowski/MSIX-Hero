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
using otor.msixhero.lib.Infrastructure;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    internal class GetVolumesReducer : BaseReducer<List<AppxVolume>>
    {
        // ReSharper disable once NotAccessedField.Local
        private readonly GetVolumes command;
        private readonly IAppxVolumeManager volumeManager;

        public GetVolumesReducer(GetVolumes command, IAppxVolumeManager volumeManager, IWritableApplicationStateManager stateManager) : base(command, stateManager)
        {
            this.command = command;
            this.volumeManager = volumeManager;
        }

        public override async Task<List<AppxVolume>> GetReduced(IInteractionService interactionService, IAppxPackageManager packageManager, CancellationToken cancellationToken = default)
        {
            var defaultVolume = await this.volumeManager.GetDefault(cancellationToken).ConfigureAwait(false);
            var allVolumes = await this.volumeManager.GetAll(cancellationToken).ConfigureAwait(false);

            if (defaultVolume != null)
            {
                defaultVolume = allVolumes.FirstOrDefault(v => string.Equals(v.PackageStorePath, defaultVolume.PackageStorePath, StringComparison.OrdinalIgnoreCase));
                if (defaultVolume != null)
                {
                    defaultVolume.IsDefault = true;
                }
            }

            return allVolumes;
        }
    }
}