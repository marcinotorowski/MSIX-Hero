using otor.msixhero.lib.Infrastructure.Helpers;

namespace otor.msixhero.lib.Domain.State
{
    public class ApplicationState : IApplicationState
    {
        public ApplicationState()
        {
            this.Packages = new PackageListState();
        }
        
        public PackageListState Packages { get; }

        IPackageListState IApplicationState.Packages => this.Packages;
        
        public bool IsElevated { get; } = UserHelper.IsAdministrator();

        public bool IsSelfElevated { get; set; }
    }
}
