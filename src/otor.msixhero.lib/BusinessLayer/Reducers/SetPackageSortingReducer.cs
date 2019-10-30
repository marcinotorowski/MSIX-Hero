using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Commands.Grid;
using otor.msixhero.lib.BusinessLayer.Events;
using otor.msixhero.lib.BusinessLayer.Infrastructure;
using otor.msixhero.lib.BusinessLayer.Infrastructure.Implementation;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    public class SetPackageSortingReducer : BaseReducer<ApplicationState>
    {
        private readonly SetPackageSorting command;

        public SetPackageSortingReducer(SetPackageSorting command, IApplicationStateManager<ApplicationState> state) : base(command, state)
        {
            this.command = command;
        }

        public override Task Reduce(CancellationToken cancellationToken)
        {
            this.StateManager.CurrentState.Packages.Sort = this.command.SortMode;
            if (this.command.Descending.HasValue)
            {
                this.StateManager.CurrentState.Packages.SortDescending = this.command.Descending.Value;
            }
            else
            {
                this.StateManager.CurrentState.Packages.SortDescending = !this.StateManager.CurrentState.Packages.SortDescending;
            }

            this.StateManager.EventAggregator.GetEvent<PackageGroupAndSortChanged>().Publish(new PackageGroupAndSortChangedPayload(this.StateManager.CurrentState.Packages.Group, this.StateManager.CurrentState.Packages.Sort, this.StateManager.CurrentState.Packages.SortDescending));
            return Task.FromResult(true);
        }
    }
}
