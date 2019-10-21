using System.Threading;
using System.Threading.Tasks;
using MSI_Hero.Domain.Actions;
using MSI_Hero.Domain.State;

namespace MSI_Hero.Domain.Reducers
{
    internal class ReloadPackagesReducer : IReducer<ApplicationState>
    {
        public Task<bool> ReduceAsync(IApplicationStateManager<ApplicationState> stateManager, CancellationToken cancellationToken)
        {
            return stateManager.Executor.ExecuteAsync(new SetPackageContext(stateManager.CurrentState.Packages.Context) { ForceReload = true }, cancellationToken);
        }
    }
}
