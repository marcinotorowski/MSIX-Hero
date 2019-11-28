using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Appx;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Appx.Packages;
using otor.msixhero.lib.Domain.Commands.Grid;
using otor.msixhero.lib.Domain.Events;
using otor.msixhero.lib.Domain.State;
using otor.msixhero.lib.Infrastructure;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    internal class SelectPackagesReducer : BaseReducer<ApplicationState, List<Package>>
    {
        private readonly SelectPackages action;

        public SelectPackagesReducer(SelectPackages action, IApplicationStateManager<ApplicationState> stateManager) : base(action, stateManager)
        {
            this.action = action;
        }

        public override Task<List<Package>> GetReduced(IInteractionService interactionService, IAppxPackageManager packageManager, CancellationToken cancellationToken = default)
        {
            IReadOnlyCollection<Package> select;
            IReadOnlyCollection<Package> deselect;

            var state = this.StateManager.CurrentState;

            switch (this.action.SelectionMode)
            {
                case SelectionMode.SelectAll:
                {
                    select = state.Packages.VisibleItems.Except(this.StateManager.CurrentState.Packages.SelectedItems).ToList();
                    deselect = new Package[0];
                    break;
                }

                case SelectionMode.UnselectAll:
                {
                    select = new Package[0];
                    deselect = state.Packages.SelectedItems.ToList();
                    break;
                }

                case SelectionMode.AddToSelection:
                    select = this.action.Selection.Except(state.Packages.SelectedItems).ToList();
                    deselect = new Package[0];
                    break;

                case SelectionMode.RemoveFromSelection:
                {
                    select = new Package[0];
                    deselect = this.action.Selection.Intersect(state.Packages.SelectedItems).ToList();
                    break;
                }

                case SelectionMode.ReplaceSelection:
                {
                    select = this.action.Selection.Except(state.Packages.SelectedItems).ToList();
                    deselect = state.Packages.SelectedItems.Except(this.action.Selection).ToList();
                    break;
                }

                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (select.Any() || deselect.Any())
            {
                state.Packages.SelectedItems.AddRange(select);
                foreach (var item in deselect)
                {
                    state.Packages.SelectedItems.Remove(item);
                }

                StateManager.EventAggregator.GetEvent<PackagesSelectionChanged>().Publish(new PackagesSelectionChangedPayLoad(@select, deselect));
            }

            return Task.FromResult(state.Packages.SelectedItems);
        }
    }
}
