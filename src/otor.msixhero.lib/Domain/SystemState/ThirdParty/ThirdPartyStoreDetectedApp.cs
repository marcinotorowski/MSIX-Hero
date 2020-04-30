namespace otor.msixhero.lib.Domain.SystemState.ThirdParty
{
    public class ThirdPartyStoreApp : ThirdPartyApp, IStoreApp
    {
        public ThirdPartyStoreApp(string appId, string name, string publisher, string website, string familyName) : base(appId, name, publisher, website)
        {
            this.PackageFamily = familyName;
        }

        public string PackageFamily { get; }
    }
}