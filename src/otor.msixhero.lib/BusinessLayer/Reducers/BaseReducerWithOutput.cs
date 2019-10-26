using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Infrastructure;
using otor.msixhero.lib.BusinessLayer.State;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    public abstract class BaseReducerWithOutput<T, TOutput> : IReducer<T, TOutput> where T : IApplicationState
    {
        public abstract Task ReduceAsync(IApplicationStateManager<T> state, CancellationToken cancellationToken);

        public abstract Task<TOutput> ReduceAndOutputAsync(IApplicationStateManager<T> state, CancellationToken cancellationToken);
    }
}