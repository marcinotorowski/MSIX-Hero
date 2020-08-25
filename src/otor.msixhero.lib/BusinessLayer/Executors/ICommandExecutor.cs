using System;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.Services;

namespace Otor.MsixHero.Lib.BusinessLayer.Executors
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