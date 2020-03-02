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

        Task Execute(BaseCommand command, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default);

        Task<TOutput> GetExecuted<TOutput>(BaseCommand<TOutput> command, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default);
    }
}