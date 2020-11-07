using System.Collections.Generic;
using System.Linq;
using Otor.MsixHero.App.Hero.Commands.Base;
using Otor.MsixHero.Appx.Volumes.Entities;

namespace Otor.MsixHero.App.Hero.Commands.Volumes
{
    public class SelectVolumesCommand : UiCommand<IList<AppxVolume>>
    {
        public SelectVolumesCommand()
        {
            this.SelectedVolumePaths = new List<string>();
        }

        public SelectVolumesCommand(string volumePath)
        {
            this.SelectedVolumePaths = new List<string> { volumePath };
        }

        public SelectVolumesCommand(IList<string> volumePaths)
        {
            this.SelectedVolumePaths = volumePaths;
        }

        public SelectVolumesCommand(params string[] volumePaths)
        {
            this.SelectedVolumePaths = volumePaths.ToList();
        }

        public SelectVolumesCommand(IEnumerable<string> manifestPaths)
        {
            this.SelectedVolumePaths = manifestPaths.ToList();
        }

        public IList<string> SelectedVolumePaths { get; }
    }
}
