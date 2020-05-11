using System;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Progress;

namespace otor.msixhero.lib.BusinessLayer.Executors
{
    public interface ICommandExecutor
    {
        Task Execute(IInteractionService interactionService, CancellationToken cancellationToken = default, IProgress<ProgressData> progressData = default);
    }

    public interface ICommandWithOutputExecutor<T> : ICommandExecutor
    {
        Task<T> ExecuteAndReturn(IInteractionService interactionService, CancellationToken cancellationToken = default, IProgress<ProgressData> progressData = default);
    }
}