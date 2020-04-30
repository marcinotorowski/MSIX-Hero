namespace otor.msixhero.lib.Domain.SystemState.ThirdParty
{
    public class ThirdPartyDetectedStoreApp : ThirdPartyStoreApp, IThirdPartyDetectedApp
    {
        public ThirdPartyDetectedStoreApp(string appId, string name, string publisher, string version, string website, string familyName) : base(appId, name, publisher, website, familyName)
        {
            this.Version = version;
        }

        public string Version { get; }
    }
}