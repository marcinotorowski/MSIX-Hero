using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Commands;
using otor.msixhero.lib.BusinessLayer.Reducers;
using otor.msixhero.lib.Ipc;

namespace otor.msixhero.lib.BusinessLayer.Infrastructure.Implementation
{
    public class CommandExecutor : ICommandExecutor
    {
        private readonly IDictionary<Type, Func<BaseCommand, IReducer<ApplicationState>>> reducerFactories = new Dictionary<Type, Func<BaseCommand, IReducer<ApplicationState>>>();
        private readonly ApplicationStateManager stateManager;
        private readonly IAppxPackageManager appxPackageManager;
        private readonly IBusyManager busyManager;
        private readonly IClientCommandRemoting clientCommandRemoting;

        public CommandExecutor(
            ApplicationStateManager stateManager, 
            IAppxPackageManager appxPackageManager, 
            IBusyManager busyManager,
            IClientCommandRemoting clientCommandRemoting)
        {
            this.stateManager = stateManager;
            this.appxPackageManager = appxPackageManager;
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

        public T Execute<T>(BaseCommand action)
        {
            try
            {
                return this.ExecuteAsync<T>(action, CancellationToken.None).Result;
            }
            catch (AggregateException e)
            {
                throw e.Flatten().GetBaseException();
            }
        }

        public async Task ExecuteAsync(BaseCommand action, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!this.reducerFactories.TryGetValue(action.GetType(), out var reducerFactory))
            {
                return;
            }

            var lazyReducer = reducerFactory(action);
            await lazyReducer.ReduceAsync(this.stateManager, cancellationToken).ConfigureAwait(false);
        }

        public async Task<T> ExecuteAsync<T>(BaseCommand action, CancellationToken cancellationToken = default(CancellationToken))
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

            return await lazyReducerOutput.ReduceAndOutputAsync(this.stateManager, cancellationToken).ConfigureAwait(false);
        }

        private void ConfigureReducers()
        {
            this.reducerFactories[typeof(SetPackageFilter)] = action => new SetPackageFilterReducer((SetPackageFilter)action);
            this.reducerFactories[typeof(SetPackageContext)] = action => new SetPackageContextReducer((SetPackageContext)action);
            this.reducerFactories[typeof(GetPackages)] = action => new GetPackagesReducer((GetPackages)action, this.appxPackageManager, this.busyManager, this.clientCommandRemoting);
            this.reducerFactories[typeof(SelectPackages)] = action => new SelectPackagesReducer((SelectPackages)action);
            this.reducerFactories[typeof(RemovePackage)] = action => new RemovePackageReducer((RemovePackage)action, this.appxPackageManager, this.busyManager, this.clientCommandRemoting);
            this.reducerFactories[typeof(GetSelectionDetails)] = action => new GetSelectionDetailsReducer((GetSelectionDetails)action, this.clientCommandRemoting);
            this.reducerFactories[typeof(GetUsersOfPackage)] = action => new GetUsersOfPackageReducer((GetUsersOfPackage)action, this.appxPackageManager, this.clientCommandRemoting);
            this.reducerFactories[typeof(SetPackageSidebarVisibility)] = action => new SetPackageSidebarVisibilityReducer((SetPackageSidebarVisibility)action);
            this.reducerFactories[typeof(MountRegistry)] = action => new MountRegistryReducer((MountRegistry)action, this.appxPackageManager, this.busyManager, this.clientCommandRemoting);
            this.reducerFactories[typeof(UnmountRegistry)] = action => new UnmountRegistryReducer((UnmountRegistry)action, this.appxPackageManager, this.busyManager, this.clientCommandRemoting);
        }
    }
}
