namespace otor.msixhero.lib.Domain.SystemState.Services
{
    public class ServiceRecommendation : IServiceRecommendation
    {
        public ServiceRecommendation(string serviceName, string displayName, string actionPrompt, bool expectedToRun, bool actual)
        {
            this.ActionPrompt = actionPrompt;
            this.Actual = actual;
            this.ExpectedToRun = expectedToRun;
            this.DisplayName = displayName;
            this.ServiceName = serviceName;
        }

        public string ActionPrompt { get; }

        public bool Actual { get; }

        public bool ExpectedToRun { get; }

        public string DisplayName { get; }

        public string ServiceName { get; }
    }
}