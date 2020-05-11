using System.Threading;
using System.Threading.Tasks;

namespace otor.msixhero.lib.Infrastructure.SelfElevation
{
    public interface ISelfElevationManagerFactory<T> where T : ISelfElevationAwareManager
    {
        Task<T> Get(SelfElevationLevel selfElevationLevel = SelfElevationLevel.AsInvoker, CancellationToken cancellationToken = default);
    }
}