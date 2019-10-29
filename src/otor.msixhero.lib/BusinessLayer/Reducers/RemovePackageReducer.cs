using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Commands;
using otor.msixhero.lib.BusinessLayer.Infrastructure;
using otor.msixhero.lib.BusinessLayer.Infrastructure.Implementation;
using otor.msixhero.lib.Ipc;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    public class RemovePackageReducer : BaseSelfElevationReducer<ApplicationState>
    {
        private readonly RemovePackage action;
        private readonly IAppxPackageManager packageManager;
        private readonly IBusyManager busyManager;

        public RemovePackageReducer(RemovePackage action, IAppxPackageManager packageManager, IBusyManager busyManager, IProcessManager processManager) : base(processManager)
        {
            this.action = action;
            this.packageManager = packageManager;
            this.busyManager = busyManager;
        }

        public override async Task ReduceAsync(IApplicationStateManager<ApplicationState> stateManager, CancellationToken cancellationToken)
        {
            var context = this.busyManager.Begin();
            context.Progress = 30;
            context.Message = "Removing " + this.action.Package.DisplayName;

            try
            {
                if (this.action.RequiresElevation && !stateManager.CurrentState.IsElevated)
                {
                    await this.packageManager.RemoveApp(this.action.Package).ConfigureAwait(false);
                }
                else
                {
                    await this.SelfElevateAndExecute(this.action, cancellationToken).ConfigureAwait(false);
                }
            }
            finally
            {
                this.busyManager.End(context);
            }
        }
    }
}
