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
    internal class SetPackageFilterClientExecutor : CommandExecutor
    {
        private readonly SetPackageFilter action;

        public SetPackageFilterClientExecutor(SetPackageFilter action, IWritableApplicationStateManager stateManager) : base(action, stateManager)
        {
            this.action = action;
        }

        public override Task Execute(IInteractionService interactionService, CancellationToken cancellationToken = default, IProgress<ProgressData> progressReporter = default)
        {
            var state = this.StateManager.CurrentState;
            var oldSearchKey = state.Packages.SearchKey;
            var newSearchKey = state.Packages.SearchKey;
            var oldFilter = state.Packages.Filter;
            var newFilter = state.Packages.Filter;
            var oldAddonsFilter = state.Packages.AddonsFilter;
            var newAddonsFilter = state.Packages.AddonsFilter;

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

            if (state.Packages.AddonsFilter != action.AddonsFilter)
            {
                state.Packages.AddonsFilter = action.AddonsFilter;
                newAddonsFilter = state.Packages.AddonsFilter;
                eventRequired = true;
            }

            cancellationToken.ThrowIfCancellationRequested();

            var toHide = new List<InstalledPackage>();
            var toShow = new List<InstalledPackage>();

            foreach (var item in state.Packages.VisibleItems)
            {
                if (!string.IsNullOrEmpty(action.SearchKey))
                {
                    if (item.DisplayName.IndexOf(action.SearchKey, StringComparison.OrdinalIgnoreCase) == -1
                        && item.DisplayPublisherName.IndexOf(action.SearchKey, StringComparison.OrdinalIgnoreCase) == -1
                        && item.Version.ToString().IndexOf(action.SearchKey, StringComparison.OrdinalIgnoreCase) == -1
                        && item.Architecture.IndexOf(action.SearchKey, StringComparison.OrdinalIgnoreCase) == -1)
                    {
                        toHide.Add(item);
                        continue;
                    }
                }

                switch (item.SignatureKind)
                {
                    case SignatureKind.Developer:
                    case SignatureKind.Unsigned:
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

                if (item.IsOptional && action.AddonsFilter == AddonsFilter.OnlyMain)
                {
                    toHide.Add(item);
                }

                if (!item.IsOptional && action.AddonsFilter == AddonsFilter.OnlyAddons)
                {
                    toHide.Add(item);
                }
            }

            foreach (var item in state.Packages.HiddenItems)
            {
                if (
                    !string.IsNullOrEmpty(action.SearchKey) && 
                    item.DisplayName.IndexOf(action.SearchKey, StringComparison.OrdinalIgnoreCase) == -1 &&
                    item.DisplayPublisherName.IndexOf(action.SearchKey, StringComparison.OrdinalIgnoreCase) == -1 &&
                    item.Version.ToString().IndexOf(action.SearchKey, StringComparison.OrdinalIgnoreCase) == -1 &&
                    item.Architecture.IndexOf(action.SearchKey, StringComparison.OrdinalIgnoreCase) == -1)
                {
                    continue;
                }

                var packageFilterOk = false;
                switch (item.SignatureKind)
                {
                    case SignatureKind.Developer:
                    case SignatureKind.Unsigned:
                    case SignatureKind.Enterprise:
                        if ((action.Filter & PackageFilter.Developer) == PackageFilter.Developer)
                        {
                            packageFilterOk = true;
                        }
                        
                        break;
                    case SignatureKind.Store:
                        if ((action.Filter & PackageFilter.Store) == PackageFilter.Store)
                        {
                            packageFilterOk = true;
                        }

                        break;
                    case SignatureKind.System:
                        if ((action.Filter & PackageFilter.System) == PackageFilter.System)
                        {
                            packageFilterOk = true;
                        }

                        break;
                }

                if (!packageFilterOk)
                {
                    continue;
                }

                if (!item.IsOptional && action.AddonsFilter != AddonsFilter.OnlyAddons)
                {
                    toShow.Add(item);
                }
                else if (item.IsOptional && action.AddonsFilter != AddonsFilter.OnlyMain)
                {
                    toShow.Add(item);
                }
            }

            var selectionRemoved = new List<InstalledPackage>();

            foreach (var item in toHide)
            {
                if (state.Packages.SelectedItems.Remove(item))
                {
                    selectionRemoved.Add(item);
                }

                state.Packages.VisibleItems.Remove(item);
                state.Packages.HiddenItems.Add(item);
            }

            foreach (var item in toShow)
            {
                state.Packages.VisibleItems.Add(item);
                state.Packages.HiddenItems.Remove(item);
            }

            if (eventRequired)
            {
                var eventToSend = new PackagesFilterChangedPayload(newFilter, oldFilter, newAddonsFilter, oldAddonsFilter, newSearchKey, oldSearchKey);
                this.StateManager.EventAggregator.GetEvent<PackagesFilterChanged>().Publish(eventToSend);
            }
            
            if (selectionRemoved.Any())
            {
                var eventToSend = new PackagesSelectionChangedPayLoad(new List<InstalledPackage>(), selectionRemoved, false);
                this.StateManager.EventAggregator.GetEvent<PackagesSelectionChanged>().Publish(eventToSend);
            }

            if (toHide.Any() || toShow.Any()) 
            {
                this.StateManager.EventAggregator.GetEvent<PackagesVisibilityChanged>().Publish(new PackagesVisibilityChangedPayLoad(toShow, toHide));
                return Task.FromResult(true);
            }
            
            return Task.FromResult(eventRequired);
        }
    }
}
