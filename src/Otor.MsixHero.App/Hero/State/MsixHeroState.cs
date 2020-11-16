namespace Otor.MsixHero.App.Hero.State
{
    public class MsixHeroState
    {
        public MsixHeroState()
        {
            this.Packages = new PackagesState();
            this.Volumes = new VolumesState();
            this.Dashboard = new DashboardState();
            this.EventViewer = new EventViewerState();
        }

        public ApplicationMode CurrentMode { get; set; }

        public PackagesState Packages { get; set; }

        public VolumesState Volumes { get; set; }

        public DashboardState Dashboard { get; set; }

        public EventViewerState EventViewer { get; set; }
    }
}
