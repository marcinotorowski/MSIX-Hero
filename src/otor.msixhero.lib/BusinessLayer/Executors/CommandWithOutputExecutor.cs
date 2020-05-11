using System;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Commands;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Progress;

namespace otor.msixhero.lib.BusinessLayer.Executors
{
    public abstract class CommandWithOutputExecutor<T> : CommandExecutor, ICommandWithOutputExecutor<T>
    {
        protected CommandWithOutputExecutor(
            CommandWithOutput<T> command, 
            IWritableApplicationStateManager stateManager) : base(command, stateManager)
        {
        }
        
        public abstract Task<T> ExecuteAndReturn(IInteractionService interactionService, 
            CancellationToken cancellationToken = default,
            IProgress<ProgressData> progressData = default);

        public override Task Execute(
            IInteractionService interactionService, 
            CancellationToken cancellationToken = default,
            IProgress<ProgressData> progressData = default)
        {
            return this.ExecuteAndReturn(interactionService, cancellationToken, progressData);
        }
    }
}