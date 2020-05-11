using System;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Commands.Packages.Grid;
using otor.msixhero.lib.Domain.Events.PackageList;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Progress;

namespace otor.msixhero.lib.BusinessLayer.Executors.Client
{
    public class SetPackageSortingClientExecutor : CommandExecutor
    {
        private readonly SetPackageSorting command;

        public SetPackageSortingClientExecutor(SetPackageSorting command, IWritableApplicationStateManager state) : base(command, state)
        {
            this.command = command;
        }

        public override Task Execute(IInteractionService interactionService, CancellationToken cancellationToken = default, IProgress<ProgressData> progressReporter = default)
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
