using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Appx.Volume;
using otor.msixhero.lib.Domain.Commands.Volumes;
using otor.msixhero.lib.Domain.Events.Volumes;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Progress;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    internal class SetVolumeFilterReducer : BaseReducer
    {
        private readonly SetVolumeFilter action;

        public SetVolumeFilterReducer(SetVolumeFilter action, IWritableApplicationStateManager stateManager) : base(action, stateManager)
        {
            this.action = action;
        }

        public override Task Reduce(IInteractionService interactionService, CancellationToken cancellationToken = default, IProgress<ProgressData> progressReporter = default)
        {
            var state = this.StateManager.CurrentState;
            var oldSearchKey = state.Volumes.SearchKey;
            var newSearchKey = state.Volumes.SearchKey;

            var eventRequired = false;

            if (state.Packages.SearchKey != action.SearchKey)
            {
                state.Volumes.SearchKey = action.SearchKey;
                newSearchKey = state.Volumes.SearchKey;
                eventRequired = true;
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (eventRequired)
            {
                var eventToSend = new VolumesFilterChangedPayload(newSearchKey, oldSearchKey);
                this.StateManager.EventAggregator.GetEvent<VolumesFilterChanged>().Publish(eventToSend);
            }

            var toHide = new List<AppxVolume>();
            var toShow = new List<AppxVolume>();

            foreach (var item in state.Volumes.VisibleItems)
            {
                if (!string.IsNullOrEmpty(action.SearchKey))
                {
                    if (
                        item.Name.IndexOf(action.SearchKey, StringComparison.OrdinalIgnoreCase) == -1 && 
                        item.PackageStorePath.IndexOf(action.SearchKey, StringComparison.OrdinalIgnoreCase) == -1 && 
                        item.Caption?.IndexOf(action.SearchKey, StringComparison.OrdinalIgnoreCase) == -1)
                    {
                        toHide.Add(item);
                    }
                }
            }

            foreach (var item in state.Volumes.HiddenItems)
            {
                if (
                    !string.IsNullOrEmpty(action.SearchKey) && 
                    item.PackageStorePath.IndexOf(action.SearchKey, StringComparison.OrdinalIgnoreCase) == -1 && 
                    item.Name.IndexOf(action.SearchKey, StringComparison.OrdinalIgnoreCase) == -1 && 
                    item.Caption?.IndexOf(action.SearchKey, StringComparison.OrdinalIgnoreCase) == -1)
                {
                    continue;
                }

                toShow.Add(item);
            }

            foreach (var item in toHide)
            {
                state.Volumes.VisibleItems.Remove(item);
                state.Volumes.HiddenItems.Add(item);
            }

            foreach (var item in toShow)
            {
                state.Volumes.VisibleItems.Add(item);
                state.Volumes.HiddenItems.Remove(item);
            }

            if (toHide.Any() || toShow.Any()) 
            {
                this.StateManager.EventAggregator.GetEvent<VolumesVisibilityChanged>().Publish(new VolumesVisibilityChangedPayLoad(toShow, toHide));
                return Task.FromResult(true);
            }
            
            return Task.FromResult(eventRequired);
        }
    }
}
