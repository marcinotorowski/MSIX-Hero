using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation.Enums;

namespace Otor.MsixHero.Infrastructure.Processes.SelfElevation
{
    public interface ISelfElevationProxyProvider<T> where T : ISelfElevationAware
    {
        Task<T> GetProxyFor(SelfElevationLevel selfElevationLevel = SelfElevationLevel.AsInvoker, CancellationToken cancellationToken = default);
    }
}