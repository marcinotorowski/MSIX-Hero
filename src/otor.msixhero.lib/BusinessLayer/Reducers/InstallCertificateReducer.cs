using System;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Appx;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Commands.Packages.Signing;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Ipc;
using otor.msixhero.lib.Infrastructure.Progress;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    public class InstallCertificateReducer : SelfElevationReducer
    {
        private readonly InstallCertificate command;
        private readonly IAppxPackageManager packageManager;

        public InstallCertificateReducer(InstallCertificate command,
            IElevatedClient elevatedClient,
            IAppxPackageManager packageManager,
            IWritableApplicationStateManager stateManager) : base(command, elevatedClient, stateManager)
        {
            this.command = command;
            this.packageManager = packageManager;
        }

        protected override Task ReduceAsCurrentUser(IInteractionService interactionService, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            return this.packageManager.InstallCertificate(this.command.FilePath, cancellationToken, progress);
        }
    }
}
