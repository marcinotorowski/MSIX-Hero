using System.Collections.Generic;
using System.Linq;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;
using Otor.MsixHero.Ui.Hero.Commands.Base;

namespace Otor.MsixHero.Ui.Hero.Commands.Packages
{
    public class SelectPackagesCommand : UiCommand<IList<InstalledPackage>>
    {
        public SelectPackagesCommand()
        {
            this.SelectedManifestPaths = new List<string>();
        }

        public SelectPackagesCommand(string manifestPaths)
        {
            this.SelectedManifestPaths = new List<string> { manifestPaths };
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
