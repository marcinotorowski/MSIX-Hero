using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.Domain.Appx.Logs;
using otor.msixhero.lib.Domain.Appx.Manifest.Full;
using otor.msixhero.lib.Domain.Appx.Packages;
using otor.msixhero.lib.Domain.Appx.Users;
using otor.msixhero.lib.Domain.Commands.Developer;
using otor.msixhero.lib.Domain.Commands.Grid;
using otor.msixhero.lib.Domain.Commands.Manager;
using otor.msixhero.lib.Domain.Commands.Signing;
using otor.msixhero.lib.Infrastructure.Interop;
using otor.msixhero.lib.Infrastructure.Ipc;
using otor.msixhero.lib.Infrastructure.Progress;

namespace otor.msixhero.lib.BusinessLayer.Appx
{
    public class RemoteAppxPackageManager : IAppxPackageManager
    {
        private readonly Client client;

        public RemoteAppxPackageManager(IProcessManager processManager)
        {
            this.client = new Client(processManager);
        }

        public Task<List<User>> GetUsersForPackage(Package package, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            return this.client.GetExecuted(new GetUsersOfPackage(package), cancellationToken, progress);
        }

        public Task<List<User>> GetUsersForPackage(string packageName, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            return this.client.GetExecuted(new GetUsersOfPackage(packageName), cancellationToken, progress);
        }

        public Task InstallCertificate(string certificateFilePath, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            return this.client.Execute(new InstallCertificate(certificateFilePath), cancellationToken, progress);
        }

        public Task Run(string packageManifestLocation, string packageFamilyName, string appId = null, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            return this.client.Execute(new RunPackage(packageFamilyName, packageManifestLocation, appId), cancellationToken, progress);
        }

        public Task MountRegistry(Package package, bool startRegedit = false, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            return client.Execute(new MountRegistry(package, startRegedit), cancellationToken, progress);
        }

        public Task MountRegistry(string packageName, string installLocation, bool startRegedit = false, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            return this.client.Execute(new MountRegistry(packageName, installLocation, startRegedit), cancellationToken, progress);
        }

        public Task UnmountRegistry(Package package, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            return this.client.Execute(new UnmountRegistry(package), cancellationToken, progress);
        }

        public Task UnmountRegistry(string packageName, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            return this.client.Execute(new UnmountRegistry(packageName), cancellationToken, progress);
        }

        public Task RunToolInContext(Package package, string toolPath, string arguments = null, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            return this.client.Execute(new RunToolInPackage(package.PackageFamilyName, package.Name, toolPath, arguments), cancellationToken, progress);
        }

        public Task RunToolInContext(string packageFamilyName, string appId, string toolPath, string arguments = null, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            return this.client.Execute(new RunToolInPackage(packageFamilyName, appId, toolPath, arguments), cancellationToken, progress);
        }

        public Task Run(Package package, string appId = null, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            return this.client.Execute(new RunPackage(package, appId), cancellationToken, progress);
        }

        public Task<RegistryMountState> GetRegistryMountState(string installLocation, string packageName, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            return this.client.GetExecuted(new GetRegistryMountState(installLocation, packageName), cancellationToken, progress);
        }

        public Task<RegistryMountState> GetRegistryMountState(Package package, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            return this.client.GetExecuted(new GetRegistryMountState(package.InstallLocation, package.Name), cancellationToken, progress);
        }

        public async Task<List<Package>> Get(PackageFindMode mode = PackageFindMode.Auto,
            CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            return await this.client.GetExecuted(new GetPackages(mode == PackageFindMode.CurrentUser ? PackageContext.CurrentUser : PackageContext.AllUsers), cancellationToken, progress).ConfigureAwait(false);
        }

        public Task<AppxPackage> Get(string packageName, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            return this.client.GetExecuted(new GetPackageDetails(packageName), cancellationToken, progress);
        }

        public Task Remove(IReadOnlyCollection<Package> packages, bool forAllUsers = false, bool preserveAppData = false, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            return this.client.Execute(new RemovePackages(forAllUsers ? PackageContext.AllUsers : PackageContext.CurrentUser, packages), cancellationToken, progress);
        }

        public async Task<List<Log>> GetLogs(int maxCount, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            return await this.client.GetExecuted(new GetLogs(maxCount), cancellationToken, progress).ConfigureAwait(false);
        }

        public Task Add(string filePath, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            return this.client.Execute(new AddPackage(filePath), cancellationToken, progress);
        }
    }
}