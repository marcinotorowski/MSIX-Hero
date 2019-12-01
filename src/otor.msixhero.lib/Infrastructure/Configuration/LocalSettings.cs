namespace otor.msixhero.lib.Infrastructure.Configuration
{
    public class LocalSettings : ILocalSettings
    {
        public LocalSettings()
        {
            this.ShowSidebar = true;
        }

        public bool ShowSidebar { get; set; }

        public bool SigningLastPfx { get; }
    }
}
