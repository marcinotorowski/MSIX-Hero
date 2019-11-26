using otor.msixhero.lib.BusinessLayer.Models.Configuration;
using otor.msixhero.lib.BusinessLayer.State;
using Prism.Events;

namespace otor.msixhero.lib.BusinessLayer.Infrastructure.Implementation
{
    public class ApplicationState : IApplicationState
    {
        public ApplicationState()
        {
            this.Packages = new PackageListState(this);
            this.LocalSettings = new LocalSettings();
        }
        
        public PackageListState Packages { get; }

        public LocalSettings LocalSettings { get; }

        IPackageListState IApplicationState.Packages => this.Packages;

        ILocalSettings IApplicationState.LocalSettings => this.LocalSettings;

        public bool IsElevated { get; } = UserHelper.IsAdministrator();

        public bool IsSelfElevated { get; set; }

        public Configuration Configuration { get; set; }
    }
}
