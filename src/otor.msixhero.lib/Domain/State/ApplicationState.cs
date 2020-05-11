using System.Threading;
using otor.msixhero.lib.Infrastructure.Helpers;

namespace otor.msixhero.lib.Domain.State
{
    public class ApplicationState : IApplicationState, IWritableApplicationState
    {
        public ApplicationState()
        {
            this.Packages = new PackageListState();
            this.Volumes = new VolumeListState();
        }
        
        public PackageListState Packages { get; }

        public VolumeListState Volumes { get; }

        IWritablePackageListState IWritableApplicationState.Packages => this.Packages;

        IPackageListState IApplicationState.Packages => this.Packages;

        IVolumeListState IApplicationState.Volumes => this.Volumes;

        IWritableVolumeListState IWritableApplicationState.Volumes => this.Volumes;

        public bool IsElevated { get; } = UserHelper.IsAdministrator();

        public bool IsSelfElevated { get; set; }

        public ApplicationMode Mode { get; set; }
    }
}
