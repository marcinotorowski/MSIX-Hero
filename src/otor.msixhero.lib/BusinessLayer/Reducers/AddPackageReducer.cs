using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Commands;
using otor.msixhero.lib.BusinessLayer.Commands.Manager;
using otor.msixhero.lib.BusinessLayer.Infrastructure;
using otor.msixhero.lib.BusinessLayer.Infrastructure.Implementation;
using otor.msixhero.ui.Services;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    public class AddPackageReducer : BaseReducer<ApplicationState>
    {
        private readonly AddPackage command;
        private readonly IAppxPackageManager packageManager;

        public AddPackageReducer(AddPackage command, IApplicationStateManager<ApplicationState> state, IAppxPackageManager packageManager) : base(command, state)
        {
            this.command = command;
            this.packageManager = packageManager;
        }

        public override Task Reduce(IInteractionService interactionService, CancellationToken cancellationToken)
        {
            return Task.Run(() => { this.packageManager.Add(this.command.FilePath); });
        }
    }
}
