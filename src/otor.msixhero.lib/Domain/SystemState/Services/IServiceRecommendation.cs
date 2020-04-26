namespace otor.msixhero.lib.Domain.SystemState.Services
{
    public interface IServiceRecommendation
    {
        string ServiceName { get; }

        string DisplayName { get; }

        string ActionPrompt { get; }

        bool ExpectedToRun { get; }

        bool Actual { get; }
    }
}
