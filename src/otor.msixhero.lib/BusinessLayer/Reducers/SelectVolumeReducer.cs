using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Appx.Volume;
using otor.msixhero.lib.Domain.Commands.Packages.Grid;
using otor.msixhero.lib.Domain.Commands.Volumes;
using otor.msixhero.lib.Domain.Events.Volumes;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Progress;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    internal class SelectVolumesReducer : BaseReducer<List<AppxVolume>>
    {
        private readonly SelectVolumes action;

        public SelectVolumesReducer(SelectVolumes action, IWritableApplicationStateManager stateManager) : base(action, stateManager)
        {
            this.action = action;
        }

        public override Task<List<AppxVolume>> GetReduced(IInteractionService interactionService, CancellationToken cancellationToken = default, IProgress<ProgressData> progressReporter = default)
        {
            IReadOnlyCollection<AppxVolume> select;
            IReadOnlyCollection<AppxVolume> deselect;

            var state = this.StateManager.CurrentState;

            switch (this.action.SelectionMode)
            {
                case SelectionMode.SelectAll:
                {
                    select = state.Volumes.VisibleItems.Where(a => a != null).Except(this.StateManager.CurrentState.Volumes.SelectedItems).ToList();
                    deselect = new AppxVolume[0];
                    break;
                }

                case SelectionMode.UnselectAll:
                {
                    select = new AppxVolume[0];
                    deselect = state.Volumes.SelectedItems.ToList();
                    break;
                }

                case SelectionMode.AddToSelection:
                    select = this.action.Selection.Where(a => a != null).Except(state.Volumes.SelectedItems).ToList();
                    deselect = new AppxVolume[0];
                    break;

                case SelectionMode.RemoveFromSelection:
                {
                    select = new AppxVolume[0];
                    deselect = this.action.Selection.Where(a => a != null).Intersect(state.Volumes.SelectedItems).ToList();
                    break;
                }

                case SelectionMode.ReplaceSelection:
                {
                    select = this.action.Selection.Where(a => a != null).Except(state.Volumes.SelectedItems).ToList();
                    deselect = state.Volumes.SelectedItems.Except(this.action.Selection).ToList();
                    break;
                }

                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (select.Any() || deselect.Any())
            {
                state.Volumes.SelectedItems.AddRange(select.Where(a => a != null));
                foreach (var item in deselect)
                {
                    state.Volumes.SelectedItems.Remove(item);
                }

                StateManager.EventAggregator.GetEvent<VolumesSelectionChanged>().Publish(new VolumesSelectionChangedPayLoad(@select, deselect, this.action.IsExplicit));
            }

            return Task.FromResult(state.Volumes.SelectedItems);
        }
    }
}
