using System.Collections.Generic;
using System.Linq;
using Prism.Regions;

namespace Otor.MsixHero.Ui.Modules.PackageList.Navigation
{
    public class PackageListNavigation
    {
        public PackageListNavigation(NavigationContext context)
        {
            var list = new List<string>();

            if (context.Parameters.TryGetValue("ManifestFiles", out IReadOnlyCollection<string> manifestFiles))
            {
                list.AddRange(manifestFiles);
            }

            this.SelectedManifests = list;
        }

        public PackageListNavigation(params string[] packageManifests)
        {
            this.SelectedManifests = packageManifests;
        }

        public PackageListNavigation()
        {
            this.SelectedManifests = new List<string>();
        }

        public IReadOnlyCollection<string> SelectedManifests { get; }

        public NavigationParameters ToParameters()
        {
            var p = new NavigationParameters();
            if (this.SelectedManifests.Any())
            {
                p.Add("ManifestFiles", this.SelectedManifests);
            }

            return p;
        }
    }
}
