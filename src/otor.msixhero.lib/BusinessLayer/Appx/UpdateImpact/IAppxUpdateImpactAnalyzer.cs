using System.Threading;
using System.Threading.Tasks;

namespace otor.msixhero.lib.BusinessLayer.Appx.UpdateImpact
{
    public interface IAppxUpdateImpactAnalyzer
    {
        Task<Domain.Appx.UpdateImpact.UpdateImpactResult> Analyze(string package1Path, string package2Path, CancellationToken cancellationToken = default);
    }
}