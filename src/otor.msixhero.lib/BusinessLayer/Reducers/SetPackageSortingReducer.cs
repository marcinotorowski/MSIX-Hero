using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Appx;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Commands.Grid;
using otor.msixhero.lib.Domain.Events;
using otor.msixhero.lib.Infrastructure;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    public class SetPackageSortingReducer : BaseReducer
    {
        private readonly SetPackageSorting command;

        public SetPackageSortingReducer(SetPackageSorting command, IWritableApplicationStateManager state) : base(command, state)
        {
            this.command = command;
        }

        public override Task Reduce(IInteractionService interactionService, IAppxPackageManager packageManager, CancellationToken cancellationToken = default)
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
