using System;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.Domain.Appx.Packages;
using otor.msixhero.lib.Domain.Commands.Packages.Developer;
using otor.msixhero.lib.Infrastructure.Ipc;
using otor.msixhero.lib.Infrastructure.Progress;

namespace otor.msixhero.lib.BusinessLayer.Managers.Registry
{
    public class ElevationProxyRegistryManager : IRegistryManager
    {
        private readonly IElevatedClient client;

        public ElevationProxyRegistryManager(IElevatedClient client)
        {
            this.client = client;
        }

        public Task MountRegistry(InstalledPackage package, bool startRegedit = false, CancellationToken cancellationToken = default,  IProgress<ProgressData> progress = default)
        {
            var cmd = new MountRegistry
            {
                PackageName = package.Name,
                InstallLocation = package.ManifestLocation,
                StartRegedit = startRegedit
            };

            return this.client.Execute(cmd, cancellationToken, progress);
        }

        public Task MountRegistry(string packageName, string installLocation, bool startRegedit = false, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var cmd = new MountRegistry
            {
                PackageName = packageName,
                InstallLocation = installLocation,
                StartRegedit = startRegedit
            };

            return this.client.Execute(cmd, cancellationToken, progress);
        }

        public Task DismountRegistry(InstalledPackage package, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var cmd = new DismountRegistry
            {
                PackageName = package.Name
            };

            return this.client.Execute(cmd, cancellationToken, progress);
        }

        public Task DismountRegistry(string packageName, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var cmd = new DismountRegistry
            {
                PackageName = packageName
            };

            return this.client.Execute(cmd, cancellationToken, progress);
        }

        public Task<RegistryMountState> GetRegistryMountState(string installLocation, string packageName, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var cmd = new GetRegistryMountState
            {
                PackageName = packageName,
                InstallLocation = installLocation
            };

            return this.client.GetExecuted(cmd, cancellationToken, progress);
        }

        public Task<RegistryMountState> GetRegistryMountState(InstalledPackage package, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var cmd = new GetRegistryMountState
            {
                PackageName = package.Name,
                InstallLocation = package.InstallLocation
            };

            return this.client.GetExecuted(cmd, cancellationToken, progress);
        }
    }
}