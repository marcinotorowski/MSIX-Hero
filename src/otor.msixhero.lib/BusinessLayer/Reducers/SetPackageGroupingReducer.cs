using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Commands.Grid;
using otor.msixhero.lib.BusinessLayer.Events;
using otor.msixhero.lib.BusinessLayer.Infrastructure;
using otor.msixhero.lib.BusinessLayer.Infrastructure.Implementation;
using otor.msixhero.lib.Services;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    public class SetPackageGroupingReducer : BaseReducer<ApplicationState>
    {
        private readonly SetPackageGrouping command;

        public SetPackageGroupingReducer(SetPackageGrouping command, IApplicationStateManager<ApplicationState> state) : base(command, state)
        {
            this.command = command;
        }

        public override Task Reduce(IInteractionService interactionService, CancellationToken cancellationToken)
        {
            this.StateManager.CurrentState.Packages.Group = this.command.GroupMode;
            this.StateManager.EventAggregator.GetEvent<PackageGroupAndSortChanged>().Publish(new PackageGroupAndSortChangedPayload(this.StateManager.CurrentState.Packages.Group, this.StateManager.CurrentState.Packages.Sort, this.StateManager.CurrentState.Packages.SortDescending));
            return Task.FromResult(true);
        }
    }
}
