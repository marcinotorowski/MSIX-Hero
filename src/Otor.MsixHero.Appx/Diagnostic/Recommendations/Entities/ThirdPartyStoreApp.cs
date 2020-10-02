namespace Otor.MsixHero.Appx.Diagnostic.Recommendations.Entities
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