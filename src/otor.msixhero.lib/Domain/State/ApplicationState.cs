using Otor.MsixHero.Infrastructure.Helpers;

namespace Otor.MsixHero.Lib.Domain.State
{
    public class ApplicationState : IApplicationState, IWritableApplicationState
    {
        public ApplicationState()
        {
            this.Volumes = new VolumeListState();
        }
        
        public VolumeListState Volumes { get; }
        
        IVolumeListState IApplicationState.Volumes => this.Volumes;

        IWritableVolumeListState IWritableApplicationState.Volumes => this.Volumes;

        public bool IsElevated { get; } = UserHelper.IsAdministrator();

        public bool IsSelfElevated { get; set; }
    }
}
