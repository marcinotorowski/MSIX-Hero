using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Appx;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Commands;
using otor.msixhero.lib.Domain.State;
using otor.msixhero.lib.Infrastructure;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    public abstract class BaseReducer<T> : IReducer<T> where T : IApplicationState
    {
        // ReSharper disable once NotAccessedField.Local
        private readonly BaseCommand command;
        protected readonly IApplicationStateManager<T> StateManager;

        protected BaseReducer(BaseCommand command, IApplicationStateManager<T> state)
        {
            this.command = command;
            this.StateManager = state;
        }

        public abstract Task Reduce(IInteractionService interactionService, IAppxPackageManager packageManager, CancellationToken cancellationToken = default);
    }

    public abstract class BaseReducer<TState, TOutput> : BaseReducer<TState>, IReducer<TState, TOutput> where TState : IApplicationState
    {
        protected BaseReducer(BaseCommand<TOutput> command, IApplicationStateManager<TState> state) : base(command, state)
        {
        }
        
        public abstract Task<TOutput> GetReduced(IInteractionService interactionService, IAppxPackageManager packageManager, CancellationToken cancellationToken = default);

        public override Task Reduce(IInteractionService interactionService, IAppxPackageManager packageManager, CancellationToken cancellationToken = default)
        {
            return this.GetReduced(interactionService, packageManager, cancellationToken);
        }
    }
}
