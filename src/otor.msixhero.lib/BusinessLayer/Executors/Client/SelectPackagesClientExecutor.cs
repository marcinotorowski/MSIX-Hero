using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Appx.Packages;
using otor.msixhero.lib.Domain.Commands.Packages.Grid;
using otor.msixhero.lib.Domain.Events.PackageList;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Progress;

namespace otor.msixhero.lib.BusinessLayer.Executors.Client
{
    internal class SelectPackagesClientExecutor : CommandWithOutputExecutor<List<InstalledPackage>>
    {
        private readonly SelectPackages action;

        public SelectPackagesClientExecutor(SelectPackages action, IWritableApplicationStateManager stateManager) : base(action, stateManager)
        {
            this.action = action;
        }

        public override Task<List<InstalledPackage>> ExecuteAndReturn(IInteractionService interactionService, CancellationToken cancellationToken = default, IProgress<ProgressData> progressReporter = default)
        {
            IReadOnlyCollection<InstalledPackage> select;
            IReadOnlyCollection<InstalledPackage> deselect;

            var state = this.StateManager.CurrentState;

            switch (this.action.SelectionMode)
            {
                case SelectionMode.SelectAll:
                {
                    select = state.Packages.VisibleItems.Except(this.StateManager.CurrentState.Packages.SelectedItems).ToList();
                    deselect = new InstalledPackage[0];
                    break;
                }

                case SelectionMode.UnselectAll:
                {
                    select = new InstalledPackage[0];
                    deselect = state.Packages.SelectedItems.ToList();
                    break;
                }

                case SelectionMode.AddToSelection:
                    select = this.action.Selection.Where(a => a != null).Except(state.Packages.SelectedItems).ToList();
                    deselect = new InstalledPackage[0];
                    break;

                case SelectionMode.RemoveFromSelection:
                {
                    select = new InstalledPackage[0];
                    deselect = this.action.Selection.Where(a => a != null).Intersect(state.Packages.SelectedItems).ToList();
                    break;
                }

                case SelectionMode.ReplaceSelection:
                {
                    select = this.action.Selection.Where(a => a != null).Except(state.Packages.SelectedItems).ToList();
                    deselect = state.Packages.SelectedItems.Except(this.action.Selection).ToList();
                    break;
                }

                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (select.Any() || deselect.Any())
            {
                state.Packages.SelectedItems.AddRange(select.Where(a => a != null));
                foreach (var item in deselect)
                {
                    state.Packages.SelectedItems.Remove(item);
                }

                StateManager.EventAggregator.GetEvent<PackagesSelectionChanged>().Publish(new PackagesSelectionChangedPayLoad(@select, deselect, this.action.IsExplicit));
            }

            return Task.FromResult(state.Packages.SelectedItems);
        }
    }
}
