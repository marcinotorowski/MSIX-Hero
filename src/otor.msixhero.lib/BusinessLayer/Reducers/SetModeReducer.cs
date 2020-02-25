using System;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Commands.Generic;
using otor.msixhero.lib.Domain.Events;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Progress;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    public class SetModeReducer : BaseReducer
    {
        private readonly SetMode command;

        public SetModeReducer(SetMode command, IWritableApplicationStateManager state) : base(command, state)
        {
            this.command = command;
        }

        public override Task Reduce(IInteractionService interactionService, CancellationToken cancellationToken = default, IProgress<ProgressData> progressReporter = default)
        {
            if (this.StateManager.CurrentState.Mode != this.command.Mode)
            {
                this.StateManager.CurrentState.Mode = this.command.Mode;
                this.StateManager.EventAggregator.GetEvent<ApplicationModeChangedEvent>().Publish(this.command.Mode);
            }

            return Task.FromResult(true);
        }
    }
}
