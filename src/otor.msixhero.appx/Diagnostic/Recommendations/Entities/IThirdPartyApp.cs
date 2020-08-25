namespace Otor.MsixHero.Appx.Diagnostic.Recommendations.Entities
{
    public interface IThirdPartyApp
    {
        string AppId { get; }

        string Name { get; }

        string Publisher { get; }

        string Website { get; }
    }
}