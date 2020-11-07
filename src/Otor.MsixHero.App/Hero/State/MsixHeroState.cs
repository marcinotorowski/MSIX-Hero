namespace Otor.MsixHero.App.Hero.State
{
    public class MsixHeroState
    {
        public MsixHeroState()
        {
            this.Packages = new PackagesState();
            this.Volumes = new VolumesState();
        }

        public PackagesState Packages { get; set; }

        public VolumesState Volumes { get; set; }

        public ApplicationMode CurrentMode { get; set; }
    }
}
