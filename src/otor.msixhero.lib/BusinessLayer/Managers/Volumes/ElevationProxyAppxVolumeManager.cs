using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.Domain.Appx.Volume;
using otor.msixhero.lib.Domain.Commands.Volumes;
using otor.msixhero.lib.Infrastructure.Ipc;
using otor.msixhero.lib.Infrastructure.Progress;
using otor.msixhero.lib.Infrastructure.SelfElevation;

namespace otor.msixhero.lib.BusinessLayer.Managers.Volumes
{
    public class ElevationProxyAppxVolumeManager : IAppxVolumeManager
    {
        private readonly ISelfElevationManagerFactory<IAppxVolumeManager> volumeManagerFactory;
        private readonly IElevatedClient client;

        public ElevationProxyAppxVolumeManager(IElevatedClient client, ISelfElevationManagerFactory<IAppxVolumeManager> volumeManagerFactory)
        {
            this.volumeManagerFactory = volumeManagerFactory;
            this.client = client;
        }

        public Task<List<AppxVolume>> GetAll(CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var cmd = new GetVolumes();
            return this.client.GetExecuted(cmd, cancellationToken, progress);
        }

        public async Task<AppxVolume> GetDefault(CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var manager = await this.volumeManagerFactory.Get(SelfElevationLevel.AsInvoker, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            return await manager.GetDefault(cancellationToken, progress).ConfigureAwait(false);
        }

        public Task<AppxVolume> Add(string drivePath, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var cmd = new AddVolume
            {
                DrivePath = drivePath
            };

            return this.client.GetExecuted(cmd, cancellationToken, progress);
        }

        public Task Delete(AppxVolume volume, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var cmd = new RemoveVolume
            {
                Name = volume.Name
            };

            return this.client.Execute(cmd, cancellationToken, progress);
        }

        public Task Delete(string name, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var cmd = new RemoveVolume
            {
                Name = name
            };

            return this.client.Execute(cmd, cancellationToken, progress);
        }

        public Task Mount(AppxVolume volume, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var mount = new MountVolume
            {
                Name = volume.Name
            };

            return this.client.Execute(mount, cancellationToken, progress);
        }

        public Task Dismount(AppxVolume volume, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var dismount = new DismountVolume
            {
                Name = volume.Name
            };

            return this.client.Execute(dismount, cancellationToken, progress);
        }

        public Task Mount(string name, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var mount = new MountVolume
            {
                Name = name
            };

            return this.client.Execute(mount, cancellationToken, progress);
        }

        public Task Dismount(string name, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var dismount = new DismountVolume
            {
                Name = name
            };

            return this.client.Execute(dismount, cancellationToken, progress);
        }

        public Task SetDefault(AppxVolume volume, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var cmd = new SetDefaultVolume
            {
                DrivePath = volume.PackageStorePath
            };

            return this.client.Execute(cmd, cancellationToken, progress);
        }

        public Task SetDefault(string drivePath, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var cmd = new SetDefaultVolume
            {
                DrivePath = drivePath
            };

            return this.client.Execute(cmd, cancellationToken, progress);
        }

        public async Task<List<AppxVolume>> GetAvailableDrivesForAppxVolume(bool onlyUnused, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var manager = await this.volumeManagerFactory.Get(SelfElevationLevel.AsInvoker, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            return await manager.GetAvailableDrivesForAppxVolume(onlyUnused, cancellationToken, progress).ConfigureAwait(false);
        }
    }
}