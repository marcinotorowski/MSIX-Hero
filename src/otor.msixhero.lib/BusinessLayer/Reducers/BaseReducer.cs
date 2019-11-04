using System;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Commands;
using otor.msixhero.lib.BusinessLayer.Infrastructure;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain;
using otor.msixhero.lib.Services;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    public abstract class BaseReducer<T> : IReducer<T> where T : IApplicationState
    {
        protected readonly IApplicationStateManager<T> StateManager;

        protected BaseReducer(BaseCommand command, IApplicationStateManager<T> state)
        {
            this.StateManager = state;
        }

        public abstract Task Reduce(IInteractionService interactionService, CancellationToken cancellationToken = default);
    }

    public abstract class BaseReducer<TState, TOutput> : BaseReducer<TState>, IReducer<TState, TOutput> where TState : IApplicationState
    {
        protected BaseReducer(BaseCommand<TOutput> command, IApplicationStateManager<TState> state) : base(command, state)
        {
        }
        
        public abstract Task<TOutput> GetReduced(IInteractionService interactionService, CancellationToken cancellationToken = default);

        public override Task Reduce(IInteractionService interactionService, CancellationToken cancellationToken = default)
        {
            return this.GetReduced(interactionService, cancellationToken);
        }
    }
}
