using System.Collections.Generic;
using System.Linq;
using Otor.MsixHero.App.Hero.Commands.Base;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;

namespace Otor.MsixHero.App.Hero.Commands.Packages
{
    public class SelectPackagesCommand : UiCommand<IList<InstalledPackage>>
    {
        public SelectPackagesCommand()
        {
            this.SelectedManifestPaths = new List<string>();
        }

        public SelectPackagesCommand(string manifestPaths)
        {
            if (manifestPaths == null)
            {
                this.SelectedManifestPaths = new List<string>();
            }
            else
            {
                this.SelectedManifestPaths = new List<string> { manifestPaths };
            }
        }

        public SelectPackagesCommand(IList<string> manifestPaths)
        {
            this.SelectedManifestPaths = manifestPaths;
        }

        public SelectPackagesCommand(params string[] manifestPaths)
        {
            this.SelectedManifestPaths = manifestPaths.ToList();
        }

        public SelectPackagesCommand(IEnumerable<string> manifestPaths)
        {
            this.SelectedManifestPaths = manifestPaths.ToList();
        }

        public IList<string> SelectedManifestPaths { get; }
    }
}
