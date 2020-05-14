using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.Domain.Appx.Manifest.Full;
using otor.msixhero.lib.Domain.Appx.Volume;
using otor.msixhero.lib.Infrastructure.Progress;
using otor.msixhero.lib.Infrastructure.SelfElevation;

namespace otor.msixhero.lib.BusinessLayer.Managers.Volumes
{
    public interface IAppxVolumeManager : ISelfElevationAwareManager
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
