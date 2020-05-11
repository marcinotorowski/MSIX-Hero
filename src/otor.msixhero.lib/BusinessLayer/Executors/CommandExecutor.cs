using System;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Commands;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Progress;

namespace otor.msixhero.lib.BusinessLayer.Executors
{
    public abstract class CommandExecutor : ICommandExecutor
    {
        // ReSharper disable once NotAccessedField.Local
        protected readonly IWritableApplicationStateManager StateManager;

        protected CommandExecutor(
            // ReSharper disable once UnusedParameter.Local
            VoidCommand command, 
            IWritableApplicationStateManager state)
        {
            this.StateManager = state;
        }

        public abstract Task Execute(
            IInteractionService interactionService, 
            CancellationToken cancellationToken = default,
            IProgress<ProgressData> progressData = default);
    }
}
