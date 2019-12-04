using otor.msixhero.lib.Infrastructure.Configuration;
using otor.msixhero.lib.Infrastructure.Helpers;

namespace otor.msixhero.lib.Domain.State
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
    }
}
