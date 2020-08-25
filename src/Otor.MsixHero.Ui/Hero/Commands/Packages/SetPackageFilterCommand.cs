using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Ui.Hero.Commands.Base;

namespace Otor.MsixHero.Ui.Hero.Commands.Packages
{
    public class SetPackageFilterCommand : UiCommand
    {
        public SetPackageFilterCommand(PackageFilter packageFilter, AddonsFilter addonFilter, string searchKey)
        {
            this.PackageFilter = packageFilter;
            this.AddonFilter = addonFilter;
            this.SearchKey = searchKey;
        }

        public PackageFilter PackageFilter { get; set; }

        public AddonsFilter AddonFilter { get; set; }

        public string SearchKey { get; set; }
    }
}
