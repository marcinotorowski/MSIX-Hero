using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Commands;
using otor.msixhero.lib.BusinessLayer.Commands.Developer;
using otor.msixhero.lib.BusinessLayer.Commands.Grid;
using otor.msixhero.lib.BusinessLayer.Commands.Manager;
using otor.msixhero.lib.BusinessLayer.Commands.UI;
using otor.msixhero.lib.BusinessLayer.Reducers;
using otor.msixhero.lib.Ipc;
using otor.msixhero.ui.Services;

namespace otor.msixhero.lib.BusinessLayer.Infrastructure.Implementation
{
    public class CommandExecutor : ICommandExecutor
    {
        private readonly IDictionary<Type, Func<BaseCommand, IReducer<ApplicationState>>> reducerFactories = new Dictionary<Type, Func<BaseCommand, IReducer<ApplicationState>>>();
        private readonly ApplicationStateManager stateManager;
        private readonly IAppxPackageManager appxPackageManager;
        private readonly IInteractionService interactionService;
        private readonly IBusyManager busyManager;
        private readonly IClientCommandRemoting clientCommandRemoting;

        public CommandExecutor(
            ApplicationStateManager stateManager, 
            IAppxPackageManager appxPackageManager, 
            IInteractionService interactionService,
            IBusyManager busyManager,
            IClientCommandRemoting clientCommandRemoting)
        {
            this.stateManager = stateManager;
            this.appxPackageManager = appxPackageManager;
            this.interactionService = interactionService;
            this.busyManager = busyManager;
            this.clientCommandRemoting = clientCommandRemoting;

            this.ConfigureReducers();
        }

        public void Execute(BaseCommand action)
        {
            try
            {
                this.ExecuteAsync(action, CancellationToken.None).GetAwaiter().GetResult();
            }
            catch (AggregateException e)
            {
                throw e.Flatten().GetBaseException();
            }
        }

        public T GetExecute<T>(BaseCommand<T> action)
        {
            try
            {
                return this.GetExecuteAsync(action, CancellationToken.None).Result;
            }
            catch (AggregateException e)
            {
                throw e.Flatten().GetBaseException();
            }
        }

        public async Task ExecuteAsync(BaseCommand action, CancellationToken cancellationToken = default)
        {
            if (!this.reducerFactories.TryGetValue(action.GetType(), out var reducerFactory))
            {
                return;
            }

            var lazyReducer = reducerFactory(action);
            await lazyReducer.Reduce(this.interactionService, cancellationToken).ConfigureAwait(false);
        }

        public async Task<T> GetExecuteAsync<T>(BaseCommand<T> action, CancellationToken cancellationToken = default)
        {
            if (!this.reducerFactories.TryGetValue(action.GetType(), out var reducerFactory))
            {
                return default;
            }

            var lazyReducer = reducerFactory(action);
            var lazyReducerOutput = lazyReducer as IReducer<ApplicationState, T>;
            if (lazyReducerOutput == null)
            {
                throw new NotSupportedException("This reducer does not support output.");
            }

            return await lazyReducerOutput.GetReduced(this.interactionService, cancellationToken).ConfigureAwait(false);
        }

        private void ConfigureReducers()
        {
            this.reducerFactories[typeof(SetPackageFilter)] = action => new SetPackageFilterReducer((SetPackageFilter)action, this.stateManager);
            this.reducerFactories[typeof(SetPackageContext)] = action => new SetPackageContextReducer((SetPackageContext)action, this.stateManager);
            this.reducerFactories[typeof(GetPackages)] = action => new GetPackagesReducer((GetPackages)action, this.stateManager, this.appxPackageManager, this.busyManager, this.clientCommandRemoting);
            this.reducerFactories[typeof(SelectPackages)] = action => new SelectPackagesReducer((SelectPackages)action, this.stateManager);
            this.reducerFactories[typeof(RemovePackages)] = action => new RemovePackageReducer((RemovePackages)action, this.stateManager, this.appxPackageManager, this.busyManager, this.clientCommandRemoting);
            this.reducerFactories[typeof(GetSelectionDetails)] = action => new GetSelectionDetailsReducer((GetSelectionDetails)action, this.stateManager, this.clientCommandRemoting);
            this.reducerFactories[typeof(GetUsersOfPackage)] = action => new GetUsersOfPackageReducer((GetUsersOfPackage)action, this.stateManager, this.appxPackageManager, this.clientCommandRemoting);
            this.reducerFactories[typeof(SetPackageSidebarVisibility)] = action => new SetPackageSidebarVisibilityReducer((SetPackageSidebarVisibility)action, this.stateManager);
            this.reducerFactories[typeof(MountRegistry)] = action => new MountRegistryReducer((MountRegistry)action, this.stateManager, this.appxPackageManager, this.busyManager, this.clientCommandRemoting);
            this.reducerFactories[typeof(UnmountRegistry)] = action => new UnmountRegistryReducer((UnmountRegistry)action, this.stateManager, this.appxPackageManager, this.busyManager, this.clientCommandRemoting);
            this.reducerFactories[typeof(SetPackageSorting)] = action => new SetPackageSortingReducer((SetPackageSorting)action, this.stateManager);
            this.reducerFactories[typeof(SetPackageGrouping)] = action => new SetPackageGroupingReducer((SetPackageGrouping)action, this.stateManager);
            this.reducerFactories[typeof(GetLogs)] = action => new GetLogsReducer((GetLogs)action, this.stateManager, this.appxPackageManager, this.clientCommandRemoting);
            this.reducerFactories[typeof(AddPackage)] = action => new AddPackageReducer((AddPackage)action, this.stateManager, this.appxPackageManager);
        }
    }
}
