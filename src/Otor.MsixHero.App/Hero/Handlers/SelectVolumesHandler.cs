using System;
using System.Collections.Generic;
using System.Linq;
using MediatR;
using Otor.MsixHero.App.Hero.Commands.Volumes;
using Otor.MsixHero.App.Hero.Executor;
using Otor.MsixHero.Appx.Volumes.Entities;

namespace Otor.MsixHero.App.Hero.Handlers
{
    public class SelectVolumesHandler : RequestHandler<SelectVolumesCommand>
    {
        private readonly IMsixHeroCommandExecutor commandExecutor;

        public SelectVolumesHandler(IMsixHeroCommandExecutor commandExecutor)
        {
            this.commandExecutor = commandExecutor;
        }

        protected override void Handle(SelectVolumesCommand request)
        {
            IList<AppxVolume> selected;
            if (!request.SelectedVolumePaths.Any())
            {
                selected = new List<AppxVolume>();
            }
            else if (request.SelectedVolumePaths.Count == 1)
            {
                selected = new List<AppxVolume>();
                var singleSelection = this.commandExecutor.ApplicationState.Volumes.AllVolumes.FirstOrDefault(a => string.Equals(a.PackageStorePath, request.SelectedVolumePaths[0], StringComparison.OrdinalIgnoreCase));

                if (singleSelection != null)
                {
                    selected.Add(singleSelection);
                }
            }
            else
            {
                selected = this.commandExecutor.ApplicationState.Volumes.AllVolumes.Where(a => request.SelectedVolumePaths.Contains(a.PackageStorePath)).ToList();
            }

            this.commandExecutor.ApplicationState.Volumes.SelectedVolumes.Clear();
            this.commandExecutor.ApplicationState.Volumes.SelectedVolumes.AddRange(selected);
        }
    }
}