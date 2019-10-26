using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Actions;
using otor.msixhero.lib.BusinessLayer.Infrastructure;
using otor.msixhero.lib.BusinessLayer.Infrastructure.Implementation;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    internal class SetPackageContextReducer : IReducer<ApplicationState>
    {
        private readonly SetPackageContext action;

        public SetPackageContextReducer(SetPackageContext action)
        {
            this.action = action;
        }

        public async Task ReduceAsync(IApplicationStateManager<ApplicationState> stateManager, CancellationToken cancellationToken)
        {
            var state = stateManager.CurrentState;

            if (state.Packages.Context == action.Context && !this.action.Force)
            {
                return;
            }

            await stateManager.CommandExecutor.ExecuteAsync(new GetPackages(action.Context), cancellationToken);
        }
    }
}