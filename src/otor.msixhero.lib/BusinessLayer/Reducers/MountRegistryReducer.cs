using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;
using otor.msixhero.lib.BusinessLayer.Commands.Developer;
using otor.msixhero.lib.BusinessLayer.Infrastructure;
using otor.msixhero.lib.BusinessLayer.Infrastructure.Implementation;
using otor.msixhero.lib.Managers;
using otor.msixhero.lib.Services;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    internal class MountRegistryReducer : SelfElevationReducer<ApplicationState>
    {
        private readonly MountRegistry action;

        public MountRegistryReducer(
            MountRegistry action,
            IApplicationStateManager<ApplicationState> stateManager) : base(action, stateManager)
        {
            this.action = action;
        }

        public override async Task Reduce(IInteractionService interactionService, IAppxPackageManager packageManager, CancellationToken cancellationToken = default)
        {
            await packageManager.MountRegistry(this.action.PackageName, this.action.InstallLocation, true, cancellationToken);
        }
    }
}
