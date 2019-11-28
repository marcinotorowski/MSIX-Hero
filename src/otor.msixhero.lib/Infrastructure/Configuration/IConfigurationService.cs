using System.Threading;
using System.Threading.Tasks;

namespace otor.msixhero.lib.Infrastructure.Configuration
{
    public interface IConfigurationService
    {
        Task<Configuration> GetConfiguration(CancellationToken token);
    }
}
