using System;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Diagnostic.Registry;
using Otor.MsixHero.Appx.Diagnostic.Registry.Enums;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;
using Otor.MsixHero.Infrastructure.Processes.Ipc;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Lib.Proxy.Diagnostic.Dto;

namespace Otor.MsixHero.Lib.Proxy.Diagnostic
{
    public class RegistryManagerElevationProxy : IRegistryManager
    {
        private readonly IElevatedClient client;

        public RegistryManagerElevationProxy(IElevatedClient client)
        {
            this.client = client;
        }

        public Task MountRegistry(InstalledPackage package, bool startRegedit = false, CancellationToken cancellationToken = default,  IProgress<ProgressData> progress = default)
        {
            var proxyObject = new MountRegistryDto
            {
                PackageName = package.Name,
                InstallLocation = package.ManifestLocation,
                StartRegedit = startRegedit
            };

            return this.client.Invoke(proxyObject, cancellationToken, progress);
        }

        public Task MountRegistry(string packageName, string installLocation, bool startRegedit = false, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var proxyObject = new MountRegistryDto
            {
                PackageName = packageName,
                InstallLocation = installLocation,
                StartRegedit = startRegedit
            };

            return this.client.Invoke(proxyObject, cancellationToken, progress);
        }

        public Task DismountRegistry(InstalledPackage package, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var proxyObject = new DismountRegistryDto
            {
                PackageName = package.Name
            };

            return this.client.Invoke(proxyObject, cancellationToken, progress);
        }

        public Task DismountRegistry(string packageName, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var proxyObject = new DismountRegistryDto
            {
                PackageName = packageName
            };

            return this.client.Invoke(proxyObject, cancellationToken, progress);
        }

        public Task<RegistryMountState> GetRegistryMountState(string installLocation, string packageName, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var proxyObject = new GetRegistryMountStateDto
            {
                PackageName = packageName,
                InstallLocation = installLocation
            };

            return this.client.Get(proxyObject, cancellationToken, progress);
        }

        public Task<RegistryMountState> GetRegistryMountState(InstalledPackage package, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var proxyObject = new GetRegistryMountStateDto
            {
                PackageName = package.Name,
                InstallLocation = package.InstallLocation
            };

            return this.client.Get(proxyObject, cancellationToken, progress);
        }
    }
}