using System.Collections.Generic;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Diagnostic.Recommendations;
using Otor.MsixHero.Appx.Diagnostic.Recommendations.Enums;

namespace Otor.MsixHero.Lib.BusinessLayer.SystemState.Services
{
    public interface IServiceRecommendationAdvisor
    {
        Task<bool> Fix(IServiceRecommendation recommendation);

        Task<bool> Revert(IServiceRecommendation recommendation);

        IEnumerable<IServiceRecommendation> Advise(AdvisorMode mode, params string[] ignoredServiceNames);
    }
}
