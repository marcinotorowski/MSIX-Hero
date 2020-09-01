using System.Collections.Generic;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Diagnostic.Recommendations.Entities;
using Otor.MsixHero.Appx.Diagnostic.Recommendations.Enums;

namespace Otor.MsixHero.Appx.Diagnostic.Recommendations
{
    public interface IServiceRecommendationAdvisor
    {
        Task<bool> Fix(IServiceRecommendation recommendation);

        Task<bool> Revert(IServiceRecommendation recommendation);

        IEnumerable<IServiceRecommendation> Advise(AdvisorMode mode, params string[] ignoredServiceNames);
    }
}
