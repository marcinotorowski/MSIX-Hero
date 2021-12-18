using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Otor.MsixHero.App.Hero.Commands.Volumes;
using Otor.MsixHero.App.Hero.Executor;
using Otor.MsixHero.Appx.Volumes;
using Otor.MsixHero.Appx.Volumes.Entities;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation.Enums;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Lib.Infrastructure.Progress;

namespace Otor.MsixHero.App.Hero.Handlers
{
    public class GetVolumesHandler : IRequestHandler<GetVolumesCommand, IList<AppxVolume>>
    {
        private readonly IMsixHeroCommandExecutor commandExecutor;
        private readonly IConfigurationService configurationService;
        private readonly ISelfElevationProxyProvider<IAppxVolumeManager> volumeManagerProvider;
        private readonly IBusyManager busyManager;

        public GetVolumesHandler(
            IMsixHeroCommandExecutor commandExecutor,
            ISelfElevationProxyProvider<IAppxVolumeManager> volumeManagerProvider,
            IBusyManager busyManager,
            IConfigurationService configurationService)
        {
            this.commandExecutor = commandExecutor;
            this.configurationService = configurationService;
            this.volumeManagerProvider = volumeManagerProvider;
            this.busyManager = busyManager;
        }

        public async Task<IList<AppxVolume>> Handle(GetVolumesCommand request, CancellationToken cancellationToken)
        {
            var context = this.busyManager.Begin(OperationType.VolumeLoading);
            try
            {
                var manager = await this.volumeManagerProvider.GetProxyFor(SelfElevationLevel.AsInvoker, cancellationToken).ConfigureAwait(false);

                var selected = this.commandExecutor.ApplicationState.Volumes.SelectedVolumes.Select(p => p.PackageStorePath).ToArray();

                var results = await manager.GetAll(cancellationToken, context).ConfigureAwait(false);
                var defaultVol = await manager.GetDefault(cancellationToken).ConfigureAwait(false);

                var findMe = defaultVol == null ? null : results.FirstOrDefault(d => d.PackageStorePath == defaultVol.PackageStorePath);
                if (findMe != null)
                {
                    findMe.IsDefault = true;
                }

                this.commandExecutor.ApplicationState.Volumes.AllVolumes.Clear();
                this.commandExecutor.ApplicationState.Volumes.AllVolumes.AddRange(results);

                if (selected.Any())
                {
                    await this.commandExecutor.Invoke(this, new SelectVolumesCommand(selected), cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    await this.commandExecutor.Invoke(this, new SelectVolumesCommand(Array.Empty<string>()), cancellationToken).ConfigureAwait(false);
                }

                return results;
            }
            finally
            {
                this.busyManager.End(context);
            }
        }
    }
}