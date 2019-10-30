using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Commands;
using otor.msixhero.lib.BusinessLayer.Infrastructure;
using otor.msixhero.lib.BusinessLayer.State;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    public abstract class BaseReducer<T> : IReducer<T> where T : IApplicationState
    {
        protected readonly IApplicationStateManager<T> StateManager;

        protected BaseReducer(BaseCommand command, IApplicationStateManager<T> state)
        {
            this.StateManager = state;
        }

        public abstract Task Reduce(CancellationToken cancellationToken);
    }

    public abstract class BaseReducer<TState, TOutput> : BaseReducer<TState>, IReducer<TState, TOutput> where TState : IApplicationState
    {
        protected BaseReducer(BaseCommand<TOutput> command, IApplicationStateManager<TState> state) : base(command, state)
        {
        }
        
        public abstract Task<TOutput> GetReduced(CancellationToken cancellationToken);

        public override Task Reduce(CancellationToken cancellationToken)
        {
            return this.GetReduced(cancellationToken);
        }
    }
}
