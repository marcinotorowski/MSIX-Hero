using Otor.MsixHero.Appx.Diagnostic.Recommendations.Enums;

namespace Otor.MsixHero.Appx.Diagnostic.Recommendations.Entities
{
    public class ServiceRecommendation : IServiceRecommendation
    {
        public ServiceRecommendation(string serviceName, string displayName, string actionPrompt, bool expectedToRun, bool actual, ServiceRecommendationType type = ServiceRecommendationType.Service)
        {
            this.ActionPrompt = actionPrompt;
            this.Actual = actual;
            this.Type = type;
            this.ExpectedToRun = expectedToRun;
            this.DisplayName = displayName;
            this.ServiceName = serviceName;
        }

        public string ActionPrompt { get; }

        public bool Actual { get; }

        public ServiceRecommendationType Type { get; }

        public bool ExpectedToRun { get; }

        public string DisplayName { get; }

        public string ServiceName { get; }
    }
}