using otor.msixhero.lib.BusinessLayer.State;

namespace otor.msixhero.lib.BusinessLayer.Infrastructure.Implementation
{
    public class LocalSettings : ILocalSettings
    {
        public LocalSettings()
        {
            this.ShowSidebar = true;
        }

        public bool ShowSidebar { get; set; }
    }
}
