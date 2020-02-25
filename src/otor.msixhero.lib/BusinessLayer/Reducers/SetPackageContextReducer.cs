using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Commands.Packages.Grid;
using otor.msixhero.lib.Infrastructure;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    internal class SetPackageContextReducer : BaseReducer
    {
        private readonly SetPackageContext action;

        public SetPackageContextReducer(SetPackageContext action, IWritableApplicationStateManager stateManager) : base(action, stateManager)
        {
            this.action = action;
        }

        public override async Task Reduce(IInteractionService interactionService, CancellationToken cancellationToken = default, IBusyManager busyManager = default)
        {
            var state = this.StateManager.CurrentState;

            if (state.Packages.Context == action.Context && !this.action.Force)
            {
                return;
            }

            await this.StateManager.CommandExecutor.GetExecuteAsync(new GetPackages(action.Context), cancellationToken).ConfigureAwait(false);
        }
    }
}