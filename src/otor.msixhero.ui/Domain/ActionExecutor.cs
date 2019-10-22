using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MSI_Hero.Domain.Actions;
using MSI_Hero.Domain.Reducers;
using MSI_Hero.Domain.State;
using MSI_Hero.Services;
using otor.msihero.lib;

namespace MSI_Hero.Domain
{
    public class ActionExecutor : IActionExecutor
    {
        private readonly IDictionary<Type, Func<IAction, IReducer<ApplicationState>>> reducerFactories = new Dictionary<Type, Func<IAction, IReducer<ApplicationState>>>();
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

        public bool Execute(IAction action)
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

        public async Task<bool> ExecuteAsync(IAction action, CancellationToken cancellationToken = default(CancellationToken))
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
            this.reducerFactories[typeof(SetPackageContext)] = action => new SetPackageContextReducer((SetPackageContext)action, this.appxPackageManager, this.busyManager);
            this.reducerFactories[typeof(ReloadPackages)] = action => new ReloadPackagesReducer();
            this.reducerFactories[typeof(SelectPackages)] = action => new SelectPackagesReducer((SelectPackages)action);
            this.reducerFactories[typeof(SetPackageSidebarVisibility)] = action => new SetPackageSidebarVisibilityReducer((SetPackageSidebarVisibility)action);
        }
    }
}
