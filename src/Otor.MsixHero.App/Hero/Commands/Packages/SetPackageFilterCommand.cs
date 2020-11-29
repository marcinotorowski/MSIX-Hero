using Otor.MsixHero.App.Hero.Commands.Base;
using Otor.MsixHero.Infrastructure.Configuration;

namespace Otor.MsixHero.App.Hero.Commands.Packages
{
    public class SetPackageFilterCommand : UiCommand
    {
        public SetPackageFilterCommand()
        {
        }

        public SetPackageFilterCommand(
            PackageFilter filter, 
            string searchKey)
        {
            this.Filter = filter;
            this.SearchKey = searchKey;
        }

        public PackageFilter Filter { get; set; }
        
        public string SearchKey { get; set; }
    }
}
