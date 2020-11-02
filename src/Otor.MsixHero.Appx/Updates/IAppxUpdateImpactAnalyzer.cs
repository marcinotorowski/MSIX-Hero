using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Updates.Entities;

namespace Otor.MsixHero.Appx.Updates
{
    public interface IAppxUpdateImpactAnalyzer
    {
        Task<UpdateImpactResult> Analyze(string package1Path, string package2Path, CancellationToken cancellationToken = default);
    }
}