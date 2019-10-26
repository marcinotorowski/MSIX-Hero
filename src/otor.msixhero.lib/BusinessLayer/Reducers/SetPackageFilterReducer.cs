using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Actions;
using otor.msixhero.lib.BusinessLayer.Events;
using otor.msixhero.lib.BusinessLayer.Infrastructure;
using otor.msixhero.lib.BusinessLayer.Infrastructure.Implementation;
using otor.msixhero.lib.BusinessLayer.State.Enums;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    internal class SetPackageFilterReducer : IReducer<ApplicationState, bool>
    {
        private readonly SetPackageFilter action;

        public SetPackageFilterReducer(SetPackageFilter action)
        {
            this.action = action;
        }

        public Task ReduceAsync(IApplicationStateManager<ApplicationState> state, CancellationToken cancellationToken)
        {
            return this.ReduceAndOutputAsync(state, cancellationToken);
        }

        public Task<bool> ReduceAndOutputAsync(IApplicationStateManager<ApplicationState> stateManager, CancellationToken cancellationToken)
        {
            var state = stateManager.CurrentState;
            var oldSearchKey = state.Packages.SearchKey;
            var newSearchKey = state.Packages.SearchKey;
            var oldFilter = state.Packages.Filter;
            var newFilter = state.Packages.Filter;

            var eventRequired = false;

            if (state.Packages.SearchKey != action.SearchKey)
            {
                state.Packages.SearchKey = action.SearchKey;
                newSearchKey = state.Packages.SearchKey;
                eventRequired = true;
            }

            if (state.Packages.Filter != action.Filter)
            {
                state.Packages.Filter = action.Filter;
                newFilter = state.Packages.Filter;
                eventRequired = true;
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (eventRequired)
            {
                var eventToSend = new PackagesFilterChangedPayload(newFilter, oldFilter, newSearchKey, oldSearchKey);
                stateManager.EventAggregator.GetEvent<PackagesFilterChanged>().Publish(eventToSend);
            }

            var toHide = new List<Package>();
            var toShow = new List<Package>();

            foreach (var item in state.Packages.VisibleItems)
            {
                if (!string.IsNullOrEmpty(action.SearchKey))
                {
                    if (item.DisplayName.IndexOf(action.SearchKey, StringComparison.OrdinalIgnoreCase) == -1 && item.DisplayPublisherName.IndexOf(action.SearchKey, StringComparison.OrdinalIgnoreCase) == -1)
                    {
                        toHide.Add(item);
                        continue;
                    }
                }

                switch (item.SignatureKind)
                {
                    case SignatureKind.Developer:
                    case SignatureKind.Enterprise:
                        if ((action.Filter & PackageFilter.Developer) != PackageFilter.Developer)
                        {
                            toHide.Add(item);
                        }

                        break;
                    case SignatureKind.Store:
                        if ((action.Filter & PackageFilter.Store) != PackageFilter.Store)
                        {
                            toHide.Add(item);
                        }

                        break;
                    case SignatureKind.System:
                        if ((action.Filter & PackageFilter.System) != PackageFilter.System)
                        {
                            toHide.Add(item);
                        }

                        break;
                }
            }

            foreach (var item in state.Packages.HiddenItems)
            {
                if (!string.IsNullOrEmpty(action.SearchKey) && item.DisplayName.IndexOf(action.SearchKey, StringComparison.OrdinalIgnoreCase) == -1 && item.DisplayPublisherName.IndexOf(action.SearchKey, StringComparison.OrdinalIgnoreCase) == -1)
                {
                    continue;
                }

                switch (item.SignatureKind)
                {
                    case SignatureKind.Developer:
                    case SignatureKind.Enterprise:
                        if ((action.Filter & PackageFilter.Developer) == PackageFilter.Developer)
                        {
                            toShow.Add(item);
                        }

                        break;
                    case SignatureKind.Store:
                        if ((action.Filter & PackageFilter.Store) == PackageFilter.Store)
                        {
                            toShow.Add(item);
                        }

                        break;
                    case SignatureKind.System:
                        if ((action.Filter & PackageFilter.System) == PackageFilter.System)
                        {
                            toShow.Add(item);
                        }

                        break;
                }
            }

            foreach (var item in toHide)
            {
                state.Packages.VisibleItems.Remove(item);
                state.Packages.HiddenItems.Add(item);
            }

            foreach (var item in toShow)
            {
                state.Packages.VisibleItems.Add(item);
                state.Packages.HiddenItems.Remove(item);
            }

            if (toHide.Any() || toShow.Any()) 
            {
                stateManager.EventAggregator.GetEvent<PackagesVisibilityChanged>().Publish(new PackagesVisibilityChangedPayLoad(toShow, toHide));
                return Task.FromResult(true);
            }
            
            return Task.FromResult(eventRequired);
        }
    }
}
