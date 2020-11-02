using System;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Diagnostic.Registry.Enums;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation;
using Otor.MsixHero.Infrastructure.Progress;

namespace Otor.MsixHero.Appx.Diagnostic.Registry
{
    public interface IRegistryManager : ISelfElevationAware
    {
        Task MountRegistry(InstalledPackage package, bool startRegedit = false, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default);

        Task MountRegistry(string packageName, string installLocation, bool startRegedit = false, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default);

        Task DismountRegistry(InstalledPackage package, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default);

        Task DismountRegistry(string packageName, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default);

        Task<RegistryMountState> GetRegistryMountState(string installLocation, string packageName, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default);

        Task<RegistryMountState> GetRegistryMountState(InstalledPackage package, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default);
    }
}
