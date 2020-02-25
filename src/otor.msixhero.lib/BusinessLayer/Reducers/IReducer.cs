using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.Infrastructure;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    internal interface IReducer
    {
        Task Reduce(IInteractionService interactionService, CancellationToken cancellationToken = default, IBusyManager busyManager = default);
    }

    internal interface IFinalizingReducer : IReducer
    {
        Task Finish(CancellationToken cancellationToken = default);
    }

    internal interface IReducer<T> : IReducer
    {
        Task<T> GetReduced(IInteractionService interactionService, CancellationToken cancellationToken = default, IBusyManager busyManager = default);
    }


    internal interface IFinalizingReducer<T> : IReducer<T>
    {
        Task Finish(T results, CancellationToken cancellationToken = default);
    }
}