using otor.msixhero.lib.Domain.SystemState.ThirdParty;

namespace otor.msixhero.ui.Modules.SystemStatus.ViewModel
{
    public class DiscoveredAppViewModel
    {
        public DiscoveredAppViewModel(ThirdPartyDetectedApp app)
        {
            this.IconId = app.AppId;
            this.Name = app.Name;
            this.Publisher = app.Publisher;
            this.Website = app.Website;
            this.Version = app.Version;
        }

        public string IconId { get; }

        public string Name { get; }

        public string Publisher { get; }
        
        public string Version { get; }

        public string Website { get; }
    }
}
