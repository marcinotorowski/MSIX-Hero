using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Appx;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Commands;
using otor.msixhero.lib.Infrastructure;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    public abstract class BaseReducer : IReducer
    {
        // ReSharper disable once NotAccessedField.Local
        private readonly BaseCommand command;
        protected readonly IWritableApplicationStateManager StateManager;

        protected BaseReducer(BaseCommand command, IWritableApplicationStateManager state)
        {
            this.command = command;
            this.StateManager = state;
        }

        public abstract Task Reduce(IInteractionService interactionService, IAppxPackageManager packageManager, CancellationToken cancellationToken = default);
    }

    public abstract class BaseReducer<T> : BaseReducer, IReducer<T>
    {
        protected BaseReducer(BaseCommand<T> command, IWritableApplicationStateManager stateManager) : base(command, stateManager)
        {
        }
        
        public abstract Task<T> GetReduced(IInteractionService interactionService, IAppxPackageManager packageManager, CancellationToken cancellationToken = default);

        public override Task Reduce(IInteractionService interactionService, IAppxPackageManager packageManager, CancellationToken cancellationToken = default)
        {
            return this.GetReduced(interactionService, packageManager, cancellationToken);
        }
    }
}
