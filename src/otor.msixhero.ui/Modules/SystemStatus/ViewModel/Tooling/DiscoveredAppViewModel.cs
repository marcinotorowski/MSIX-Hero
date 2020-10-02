using Otor.MsixHero.Appx.Diagnostic.Recommendations.Entities;

namespace Otor.MsixHero.Ui.Modules.SystemStatus.ViewModel.Tooling
{
    public class DiscoveredAppViewModel
    {
        public DiscoveredAppViewModel(IThirdPartyApp app, DiscoveredAppViewModelStatus status = DiscoveredAppViewModelStatus.Unknown)
        {
            this.Status = status;
            this.IconId = app.AppId;
            this.Name = app.Name;
            this.Publisher = app.Publisher;
            this.Website = app.Website;
        }

        public DiscoveredAppViewModel(IThirdPartyDetectedApp app, DiscoveredAppViewModelStatus status = DiscoveredAppViewModelStatus.Unknown)
        {
            this.Status = status;
            this.IconId = app.AppId;
            this.Name = app.Name;
            this.Publisher = app.Publisher;
            this.Website = app.Website;
            this.Version = app.Version;
        }

        public DiscoveredAppViewModelStatus Status { get; }

        public string IconId { get; }

        public string Name { get; }

        public string Publisher { get; }
        
        public string Version { get; }

        public string Website { get; }
    }
}
