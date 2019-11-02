using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Commands;
using otor.msixhero.lib.BusinessLayer.Commands.Manager;
using otor.msixhero.lib.BusinessLayer.Infrastructure;
using otor.msixhero.lib.BusinessLayer.Infrastructure.Implementation;
using otor.msixhero.lib.Managers;
using otor.msixhero.ui.Services;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    public class AddPackageReducer : BaseReducer<ApplicationState>
    {
        private readonly AddPackage command;
        private readonly IAppxPackageManager packageManager;
        private readonly IBusyManager busyManager;

        public AddPackageReducer(AddPackage command, IApplicationStateManager<ApplicationState> state, IAppxPackageManager packageManager, IBusyManager busyManager) : base(command, state)
        {
            this.command = command;
            this.packageManager = packageManager;
            this.busyManager = busyManager;
        }

        public override async Task Reduce(IInteractionService interactionService, CancellationToken cancellationToken = default)
        {
            await this.busyManager.Execute(progress => this.packageManager.Add(this.command.FilePath, cancellationToken, progress));
        }
    }
}
