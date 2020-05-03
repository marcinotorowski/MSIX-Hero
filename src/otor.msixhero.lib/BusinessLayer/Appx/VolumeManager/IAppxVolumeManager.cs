using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.Domain.Appx.Volume;
using otor.msixhero.lib.Infrastructure.Progress;

namespace otor.msixhero.lib.BusinessLayer.Appx.VolumeManager
{
    public interface IAppxVolumeManager
    {
        Task<List<AppxVolume>> GetAll(CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default);
        
        Task<AppxVolume> GetDefault(CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default);

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
    }
}
