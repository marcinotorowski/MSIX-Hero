namespace otor.msixhero.lib.Domain.SystemState.Services
{
    public class ServiceRecommendation : IServiceRecommendation
    {
        public ServiceRecommendation(string name, string actionPrompt, bool expectedToRun, bool actual)
        {
            this.ActionPrompt = actionPrompt;
            this.Actual = actual;
            this.ExpectedToRun = expectedToRun;
            this.Name = name;
        }

        public string ActionPrompt { get; }

        public bool Actual { get; }

        public bool ExpectedToRun { get; }

        public string Name { get; }
    }
}