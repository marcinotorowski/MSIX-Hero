using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualBasic.CompilerServices;
using MSI_Hero.Domain.Actions;
using MSI_Hero.Domain.Events;
using MSI_Hero.Domain.State;
using otor.msihero.lib;

namespace MSI_Hero.Domain.Reducers
{
    internal class SelectPackagesReducer : IReducer<ApplicationState>
    {
        private readonly SelectPackages action;

        public SelectPackagesReducer(SelectPackages action)
        {
            this.action = action;
        }

        public Task<bool> ReduceAsync(IApplicationStateManager<ApplicationState> state, CancellationToken cancellationToken)
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

            if (!select.Any() && !deselect.Any())
            {
                return Task.FromResult(false);
            }

            state.CurrentState.Packages.SelectedItems.AddRange(select);
            foreach (var item in deselect)
            {
                state.CurrentState.Packages.SelectedItems.Remove(item);
            }

            state.EventAggregator.GetEvent<PackagesSelectionChanged>().Publish(new PackagesSelectionChangedPayLoad(@select, deselect));
            return Task.FromResult(true);
        }
    }
}
