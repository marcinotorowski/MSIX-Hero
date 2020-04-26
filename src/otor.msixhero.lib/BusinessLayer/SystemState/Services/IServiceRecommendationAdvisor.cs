using System.Collections.Generic;
using System.Threading.Tasks;
using otor.msixhero.lib.Domain.SystemState;
using otor.msixhero.lib.Domain.SystemState.Services;

namespace otor.msixhero.lib.BusinessLayer.SystemState.Services
{
    public interface IServiceRecommendationAdvisor
    {
        Task<bool> Fix(IServiceRecommendation recommendation);

        Task<bool> Revert(IServiceRecommendation recommendation);

        IEnumerable<IServiceRecommendation> Advise(AdvisorMode mode, params string[] ignoredServiceNames);
    }
}
