using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Otor.MsixHero.App.Hero.Commands.Volumes;
using Otor.MsixHero.App.Hero.Executor;
using Otor.MsixHero.App.Mvvm.Progress;
using Otor.MsixHero.Appx.Volumes;
using Otor.MsixHero.Appx.Volumes.Entities;

namespace Otor.MsixHero.App.Hero.Handlers
{
    public class GetVolumesHandler : IRequestHandler<GetVolumesCommand, IList<AppxVolume>>
    {
        private readonly IMsixHeroCommandExecutor _commandExecutor;
        private readonly IAppxVolumeService _volumeServiceProvider;
        private readonly IBusyManager _busyManager;

        public GetVolumesHandler(
            IMsixHeroCommandExecutor commandExecutor,
            IAppxVolumeService volumeServiceProvider,
            IBusyManager busyManager)
        {
            this._commandExecutor = commandExecutor;
            this._volumeServiceProvider = volumeServiceProvider;
            this._busyManager = busyManager;
        }

        public async Task<IList<AppxVolume>> Handle(GetVolumesCommand request, CancellationToken cancellationToken)
        {
            var context = this._busyManager.Begin(OperationType.VolumeLoading);
            try
            {
                var manager = this._volumeServiceProvider;

                var selected = this._commandExecutor.ApplicationState.Volumes.SelectedVolumes.Select(p => p.PackageStorePath).ToArray();

                var results = await manager.GetAll(cancellationToken, context).ConfigureAwait(false);
                var defaultVol = await manager.GetDefault(cancellationToken).ConfigureAwait(false);

                var findMe = defaultVol == null ? null : results.FirstOrDefault(d => d.PackageStorePath == defaultVol.PackageStorePath);
                if (findMe != null)
                {
                    findMe.IsDefault = true;
                }

                this._commandExecutor.ApplicationState.Volumes.AllVolumes.Clear();
                this._commandExecutor.ApplicationState.Volumes.AllVolumes.AddRange(results);

                if (selected.Any())
                {
                    await this._commandExecutor.Invoke(this, new SelectVolumesCommand(selected), cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    await this._commandExecutor.Invoke(this, new SelectVolumesCommand(Array.Empty<string>()), cancellationToken).ConfigureAwait(false);
                }

                return results;
            }
            finally
            {
                this._busyManager.End(context);
            }
        }
    }
}