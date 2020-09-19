using Otor.MsixHero.Appx.Packaging.Manifest.Enums;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Ui.Hero.Commands.Base;

namespace Otor.MsixHero.Ui.Hero.Commands.Packages
{
    public class SetPackageFilterCommand : UiCommand
    {
        public SetPackageFilterCommand(
            PackageFilter packageFilter, 
            string searchKey)
        {
            this.PackageFilter = packageFilter;
            this.SearchKey = searchKey;
        }

        public SetPackageFilterCommand()
        {
        }
        
        public PackageFilter PackageFilter { get; set; }
        
        public string SearchKey { get; set; }
    }
}
