using System.Threading;
using System.Threading.Tasks;

namespace otor.msixhero.lib.Infrastructure.Configuration
{
    public interface IConfigurationService
    {
        Task<Configuration> GetCurrentConfigurationAsync(bool preferCached = true, CancellationToken token = default);

        Configuration GetCurrentConfiguration(bool preferCached = true);

        Task SetCurrentConfigurationAsync(Configuration configuration, CancellationToken cancellationToken = default);

        void SetCurrentConfiguration(Configuration configuration);
    }
}
