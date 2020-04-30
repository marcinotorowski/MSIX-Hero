namespace otor.msixhero.lib.Domain.SystemState.ThirdParty
{
    public class ThirdPartyDetectedApp : ThirdPartyApp, IThirdPartyDetectedApp
    {
        public ThirdPartyDetectedApp(string appId, string name, string publisher, string version, string website) : base(appId, name, publisher, website)
        {
            this.Version = version;
        }

        public string Version { get; }
    }
}
