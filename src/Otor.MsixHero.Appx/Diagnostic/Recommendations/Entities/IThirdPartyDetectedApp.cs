namespace Otor.MsixHero.Appx.Diagnostic.Recommendations.Entities
{
    public interface IThirdPartyDetectedApp : IThirdPartyApp
    {
        string Version { get; }
    }
}