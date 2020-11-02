using System.Threading;
using System.Threading.Tasks;

namespace Otor.MsixHero.Infrastructure.Services
{
    public interface IConfigurationService
    {
        Task<Configuration.Configuration> GetCurrentConfigurationAsync(bool preferCached = true, CancellationToken token = default);

        Configuration.Configuration GetCurrentConfiguration(bool preferCached = true);

        Task SetCurrentConfigurationAsync(Configuration.Configuration configuration, CancellationToken cancellationToken = default);

        void SetCurrentConfiguration(Configuration.Configuration configuration);
    }
}
