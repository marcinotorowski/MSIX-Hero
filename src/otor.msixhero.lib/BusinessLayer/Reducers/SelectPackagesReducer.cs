using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Commands;
using otor.msixhero.lib.BusinessLayer.Events;
using otor.msixhero.lib.BusinessLayer.Infrastructure;
using otor.msixhero.lib.BusinessLayer.Infrastructure.Implementation;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    internal class SelectPackagesReducer : IReducer<ApplicationState, IList<Package>>
    {
        private readonly SelectPackages action;

        public SelectPackagesReducer(SelectPackages action)
        {
            this.action = action;
        }

        public Task ReduceAsync(IApplicationStateManager<ApplicationState> state, CancellationToken cancellationToken)
        {

            IReadOnlyCollection<Package> select;
            IReadOnlyCollection<Package> deselect;
            switch (this.action.SelectionMode)
            {
                case SelectionMode.SelectAll:
                {
                    select = state.CurrentState.Packages.VisibleItems.Except(state.CurrentState.Packages.SelectedItems).ToList();
                    deselect = new Package[0];
                    break;
                }

                case SelectionMode.UnselectAll:
                {
                    select = new Package[0];
                    deselect = state.CurrentState.Packages.SelectedItems.ToList();
                    break;
                }

                case SelectionMode.AddToSelection:
                    select = this.action.Selection.Except(state.CurrentState.Packages.SelectedItems).ToList();
                    deselect = new Package[0];
                    break;

                case SelectionMode.RemoveFromSelection:
                {
                    select = new Package[0];
                    deselect = this.action.Selection.Intersect(state.CurrentState.Packages.SelectedItems).ToList();
                    break;
                }

                case SelectionMode.ReplaceSelection:
                {
                    select = this.action.Selection.Except(state.CurrentState.Packages.SelectedItems).ToList();
                    deselect = state.CurrentState.Packages.SelectedItems.Except(this.action.Selection).ToList();
                    break;
                }

                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (select.Any() || deselect.Any())
            {
                state.CurrentState.Packages.SelectedItems.AddRange(select);
                foreach (var item in deselect)
                {
                    state.CurrentState.Packages.SelectedItems.Remove(item);
                }

                state.EventAggregator.GetEvent<PackagesSelectionChanged>().Publish(new PackagesSelectionChangedPayLoad(@select, deselect));
            }

            return Task.FromResult(true);
        }

        public async Task<IList<Package>> ReduceAndOutputAsync(IApplicationStateManager<ApplicationState> state, CancellationToken cancellationToken)
        {
            await this.ReduceAsync(state, cancellationToken).ConfigureAwait(false);
            return state.CurrentState.Packages.SelectedItems;
        }
    }
}
