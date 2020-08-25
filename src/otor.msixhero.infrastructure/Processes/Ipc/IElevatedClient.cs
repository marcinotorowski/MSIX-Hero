using System;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Infrastructure.Progress;

namespace Otor.MsixHero.Infrastructure.Processes.Ipc
{
    public interface IElevatedClient
    {
        Task<bool> Test(CancellationToken cancellationToken = default);

        Task Invoke(IProxyObject command, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default);

        Task<TOutput> Get<TOutput>(IProxyObjectWithOutput<TOutput> command, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default);
    }
}