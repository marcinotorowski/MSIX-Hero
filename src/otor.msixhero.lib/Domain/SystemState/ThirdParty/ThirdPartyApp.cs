namespace otor.msixhero.lib.Domain.SystemState.ThirdParty
{
    public class ThirdPartyApp : IThirdPartyApp
    {
        public ThirdPartyApp(string appId, string name, string publisher, string website)
        {
            this.AppId = appId;
            this.Name = name;
            this.Publisher = publisher;
            this.Website = website;
        }

        public string AppId { get; }

        public string Name { get; }

        public string Publisher { get; }

        public string Website { get; }
    }
}