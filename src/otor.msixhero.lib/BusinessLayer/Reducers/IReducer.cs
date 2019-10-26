using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Infrastructure;
using otor.msixhero.lib.BusinessLayer.State;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    internal interface IReducer<in T> where T : IApplicationState
    {
        Task ReduceAsync(IApplicationStateManager<T> state, CancellationToken cancellationToken);
    }

    internal interface IReducer<in T, TOutput> : IReducer<T> where T : IApplicationState
    {
        Task<TOutput> ReduceAndOutputAsync(IApplicationStateManager<T> state, CancellationToken cancellationToken);
    }
}