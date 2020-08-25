namespace Otor.MsixHero.Appx.Diagnostic.Recommendations.Entities
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
