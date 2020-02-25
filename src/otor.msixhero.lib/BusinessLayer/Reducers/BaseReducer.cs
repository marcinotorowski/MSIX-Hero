using System;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Commands;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Progress;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    public abstract class BaseReducer : IReducer
    {
        // ReSharper disable once NotAccessedField.Local
        protected readonly IWritableApplicationStateManager StateManager;

        protected BaseReducer(
            // ReSharper disable once UnusedParameter.Local
            BaseCommand command, 
            IWritableApplicationStateManager state)
        {
            this.StateManager = state;
        }

        public abstract Task Reduce(
            IInteractionService interactionService, 
            CancellationToken cancellationToken = default,
            IProgress<ProgressData> progressData = default);
    }

    public abstract class BaseReducer<T> : BaseReducer, IReducer<T>
    {
        protected BaseReducer(
            BaseCommand<T> command, 
            IWritableApplicationStateManager stateManager) : base(command, stateManager)
        {
        }
        
        public abstract Task<T> GetReduced(IInteractionService interactionService, 
            CancellationToken cancellationToken = default,
            IProgress<ProgressData> progressData = default);

        public override Task Reduce(
            IInteractionService interactionService, 
            CancellationToken cancellationToken = default,
            IProgress<ProgressData> progressData = default)
        {
            return this.GetReduced(interactionService, cancellationToken, progressData);
        }
    }
}
