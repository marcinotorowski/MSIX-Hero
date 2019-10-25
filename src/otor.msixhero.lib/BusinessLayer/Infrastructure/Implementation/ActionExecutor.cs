using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Actions;
using otor.msixhero.lib.BusinessLayer.Reducers;

namespace otor.msixhero.lib.BusinessLayer.Infrastructure.Implementation
{
    public class ActionExecutor : IActionExecutor
    {
        private readonly IDictionary<Type, Func<BaseAction, IReducer<ApplicationState>>> reducerFactories = new Dictionary<Type, Func<BaseAction, IReducer<ApplicationState>>>();
        private readonly ApplicationStateManager stateManager;
        private readonly IAppxPackageManager appxPackageManager;
        private readonly IBusyManager busyManager;

        public ActionExecutor(ApplicationStateManager stateManager, IAppxPackageManager appxPackageManager, IBusyManager busyManager)
        {
            this.stateManager = stateManager;
            this.appxPackageManager = appxPackageManager;
            this.busyManager = busyManager;

            this.ConfigureReducers();
        }

        public bool Execute(BaseAction action)
        {
            try
            {
                return this.ExecuteAsync(action, CancellationToken.None).Result;
            }
            catch (AggregateException e)
            {
                throw e.Flatten().GetBaseException();
            }
        }

        public async Task<bool> ExecuteAsync(BaseAction action, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!this.reducerFactories.TryGetValue(action.GetType(), out var reducerFactory))
            {
                return false;
            }

            var lazyReducer = reducerFactory(action);
            return await lazyReducer.ReduceAsync(this.stateManager, cancellationToken).ConfigureAwait(false);
        }
        
        private void ConfigureReducers()
        {
            this.reducerFactories[typeof(SetPackageFilter)] = action => new SetPackageFilterReducer((SetPackageFilter)action, this.appxPackageManager, this.busyManager);
            this.reducerFactories[typeof(SetPackageContext)] = action => new SetPackageContextReducer((SetPackageContext)action);
            this.reducerFactories[typeof(GetPackages)] = action => new ReloadPackagesReducer((GetPackages)action, this.appxPackageManager, this.busyManager);
            this.reducerFactories[typeof(SelectPackages)] = action => new SelectPackagesReducer((SelectPackages)action);
            this.reducerFactories[typeof(SetPackageSidebarVisibility)] = action => new SetPackageSidebarVisibilityReducer((SetPackageSidebarVisibility)action);
            this.reducerFactories[typeof(MountRegistry)] = action => new MountRegistryReducer((MountRegistry)action, this.appxPackageManager, this.busyManager);
            this.reducerFactories[typeof(UnmountRegistry)] = action => new UnmountRegistryReducer((UnmountRegistry)action, this.appxPackageManager, this.busyManager);
        }
    }
}
