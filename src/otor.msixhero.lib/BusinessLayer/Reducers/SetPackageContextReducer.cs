using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Commands;
using otor.msixhero.lib.BusinessLayer.Commands.Grid;
using otor.msixhero.lib.BusinessLayer.Infrastructure;
using otor.msixhero.lib.BusinessLayer.Infrastructure.Implementation;
using otor.msixhero.lib.Managers;
using otor.msixhero.lib.Services;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    internal class SetPackageContextReducer : BaseReducer<ApplicationState>
    {
        private readonly SetPackageContext action;

        public SetPackageContextReducer(SetPackageContext action, IApplicationStateManager<ApplicationState> stateManager) : base(action, stateManager)
        {
            this.action = action;
        }

        public override async Task Reduce(IInteractionService interactionService, IAppxPackageManager packageManager, CancellationToken cancellationToken = default)
        {
            var state = this.StateManager.CurrentState;

            if (state.Packages.Context == action.Context && !this.action.Force)
            {
                return;
            }

            await this.StateManager.CommandExecutor.ExecuteAsync(new GetPackages(action.Context), cancellationToken);
        }
    }
}