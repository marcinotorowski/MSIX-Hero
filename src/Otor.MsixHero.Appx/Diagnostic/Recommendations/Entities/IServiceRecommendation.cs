namespace Otor.MsixHero.Appx.Diagnostic.Recommendations
{
    public interface IServiceRecommendation
    {
        string ServiceName { get; }

        string DisplayName { get; }

        string ActionPrompt { get; }

        bool ExpectedToRun { get; }

        bool Actual { get; }

        ServiceRecommendationType Type { get; }
    }
}
