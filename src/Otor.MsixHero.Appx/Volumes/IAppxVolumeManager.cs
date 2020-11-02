using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;
using Otor.MsixHero.Appx.Volumes.Entities;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation;
using Otor.MsixHero.Infrastructure.Progress;

namespace Otor.MsixHero.Appx.Volumes
{
    public interface IAppxVolumeManager : ISelfElevationAware
    {
        Task<List<AppxVolume>> GetAll(CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default);
        
        Task<AppxVolume> GetDefault(CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default);
        
        Task<AppxVolume> GetVolumeForPath(string path, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default);

        Task<AppxVolume> Add(string drivePath, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default);

        Task Delete(AppxVolume volume, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default);

        Task Delete(string name, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default);

        Task Mount(AppxVolume volume, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default);

        Task Dismount(AppxVolume volume, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default);

        Task Mount(string name, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default);

        Task Dismount(string name, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default);

        Task SetDefault(AppxVolume volume, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default);

        Task SetDefault(string drivePath, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default);

        Task<List<AppxVolume>> GetAvailableDrivesForAppxVolume(bool onlyUnused, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default);

        Task MovePackageToVolume(string volumePackagePath, string packageFullName, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default);

        Task MovePackageToVolume(AppxVolume volume, AppxPackage package, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default);
    }
}
