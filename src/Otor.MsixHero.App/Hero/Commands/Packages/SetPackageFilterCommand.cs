using Otor.MsixHero.App.Hero.Commands.Base;
using Otor.MsixHero.Infrastructure.Configuration;

namespace Otor.MsixHero.App.Hero.Commands.Packages
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
