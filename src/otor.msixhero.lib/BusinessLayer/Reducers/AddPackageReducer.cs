using System;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Appx;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Commands.Packages.Manager;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Ipc;
using otor.msixhero.lib.Infrastructure.Progress;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    public class AddPackageReducer : SelfElevationReducer
    {
        private readonly AddPackage command;
        private readonly IAppxPackageManager packageManager;

        public AddPackageReducer(
            AddPackage command, 
            IElevatedClient elevatedClient,
            IAppxPackageManager packageManager, 
            IWritableApplicationStateManager stateManager) : base(command, elevatedClient, stateManager)
        {
            this.command = command;
            this.packageManager = packageManager;
        }

        protected override Task ReduceAsCurrentUser(IInteractionService interactionService, CancellationToken cancellationToken = default, IProgress<ProgressData> progressData = default)
        {
            AddPackageOptions opts = 0;

            if (this.command.AllUsers)
            {
                opts |= AddPackageOptions.AllUsers;
            }

            if (this.command.KillRunningApps)
            {
                opts |= AddPackageOptions.KillRunningApps;
            }

            if (this.command.AllowDowngrade)
            {
                opts |= AddPackageOptions.AllowDowngrade;
            }

            return this.packageManager.Add(this.command.FilePath, opts, cancellationToken, progressData);
        }
    }
}
