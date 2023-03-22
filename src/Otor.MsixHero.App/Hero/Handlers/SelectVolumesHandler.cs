using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Otor.MsixHero.App.Hero.Commands.Volumes;
using Otor.MsixHero.App.Hero.Executor;
using Otor.MsixHero.Appx.Volumes.Entities;

namespace Otor.MsixHero.App.Hero.Handlers
{
    public class SelectVolumesHandler : IRequestHandler<SelectVolumesCommand>
    {
        private readonly IMsixHeroCommandExecutor _commandExecutor;

        public SelectVolumesHandler(IMsixHeroCommandExecutor commandExecutor)
        {
            this._commandExecutor = commandExecutor;
        }

        Task IRequestHandler<SelectVolumesCommand>.Handle(SelectVolumesCommand request, CancellationToken cancellationToken)
        {
            IList<AppxVolume> selected;
            if (!request.SelectedVolumePaths.Any())
            {
                selected = new List<AppxVolume>();
            }
            else if (request.SelectedVolumePaths.Count == 1)
            {
                selected = new List<AppxVolume>();
                var singleSelection = this._commandExecutor.ApplicationState.Volumes.AllVolumes.FirstOrDefault(a => string.Equals(a.PackageStorePath, request.SelectedVolumePaths[0], StringComparison.OrdinalIgnoreCase));

                if (singleSelection != null)
                {
                    selected.Add(singleSelection);
                }
            }
            else
            {
                selected = this._commandExecutor.ApplicationState.Volumes.AllVolumes.Where(a => request.SelectedVolumePaths.Contains(a.PackageStorePath)).ToList();
            }

            this._commandExecutor.ApplicationState.Volumes.SelectedVolumes.Clear();
            this._commandExecutor.ApplicationState.Volumes.SelectedVolumes.AddRange(selected);

            return Task.CompletedTask;
        }
    }
}