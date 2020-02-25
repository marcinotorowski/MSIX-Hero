using System;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Progress;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    internal interface IReducer
    {
        Task Reduce(IInteractionService interactionService, CancellationToken cancellationToken = default, IProgress<ProgressData> progressData = default);
    }

    internal interface IFinalizingReducer : IReducer
    {
        Task Finish(CancellationToken cancellationToken = default);
    }

    internal interface IReducer<T> : IReducer
    {
        Task<T> GetReduced(IInteractionService interactionService, CancellationToken cancellationToken = default, IProgress<ProgressData> progressData = default);
    }


    internal interface IFinalizingReducer<T> : IReducer<T>
    {
        Task Finish(T results, CancellationToken cancellationToken = default);
    }
}