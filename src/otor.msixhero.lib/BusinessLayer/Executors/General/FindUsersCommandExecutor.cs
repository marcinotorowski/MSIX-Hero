using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Appx.Manifest;
using otor.msixhero.lib.BusinessLayer.Appx.Manifest.FileReaders;
using otor.msixhero.lib.BusinessLayer.Managers.Packages;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Appx.Users;
using otor.msixhero.lib.Domain.Commands.Packages.Grid;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Progress;
using otor.msixhero.lib.Infrastructure.SelfElevation;
using otor.msixhero.lib.Interop;

namespace otor.msixhero.lib.BusinessLayer.Executors.General
{
    internal class FindUsersCommandExecutor : CommandWithOutputExecutor<List<User>>
    {
        private readonly FindUsers action;
        private readonly ISelfElevationManagerFactory<IAppxPackageManager> packageManagerFactory;

        public FindUsersCommandExecutor(FindUsers action, ISelfElevationManagerFactory<IAppxPackageManager> packageManagerFactory, IWritableApplicationStateManager applicationStateManager) : base(action, applicationStateManager)
        {
            this.action = action;
            this.packageManagerFactory = packageManagerFactory;
        }

        public override async Task<List<User>> ExecuteAndReturn(IInteractionService interactionService, CancellationToken cancellationToken = default, IProgress<ProgressData> progressData = default)
        {
            if (!this.action.ForceElevation && !this.StateManager.CurrentState.IsElevated && !this.StateManager.CurrentState.IsSelfElevated)
            {
                // if there is no indication that we can run in UAC don't even try
                return null;
            }

            var source = this.action.Source;
            // ReSharper disable once InvertIf
            if (Uri.TryCreate(source, UriKind.Absolute, out _))
            {
                using IAppxFileReader reader = new FileInfoFileReaderAdapter(source);
                var appxManifestReader = new AppxManifestReader();
                var pkg = await appxManifestReader.Read(reader, cancellationToken).ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();

                source = AppxPackaging.GetPackageFullName(pkg.Name, pkg.Publisher, pkg.ProcessorArchitecture, pkg.Version, pkg.ResourceId);
            }

            var manager = await this.packageManagerFactory.Get(SelfElevationLevel.AsAdministrator, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();

            return await manager.GetUsersForPackage(source, this.action.ForceElevation, cancellationToken, progressData).ConfigureAwait(false);
        }
    }
}
