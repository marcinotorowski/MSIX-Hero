using System;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.Domain.Commands;
using otor.msixhero.lib.Infrastructure.Progress;

namespace otor.msixhero.lib.Infrastructure.Ipc
{
    public interface IElevatedClient
    {
        Task<bool> Test(CancellationToken cancellationToken = default);

        Task Execute(VoidCommand command, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default);

        Task<TOutput> GetExecuted<TOutput>(CommandWithOutput<TOutput> command, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default);
    }
}