using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Volumes;
using Otor.MsixHero.Appx.Volumes.Entities;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation.Enums;

namespace Otor.MsixHero.App.Controls.PackageExpert.ViewModels.Items
{
    public class PackageDriveViewModel : NotifyPropertyChanged
    {
        private readonly ISelfElevationProxyProvider<IAppxVolumeManager> volumeManagerFactory;

        public PackageDriveViewModel(ISelfElevationProxyProvider<IAppxVolumeManager> volumeManagerFactory)
        {
            this.volumeManagerFactory = volumeManagerFactory;
        }

        public async Task Load(string filePath)
        {
            var disk = await this.GetDisk(filePath).ConfigureAwait(true);
            this.Disk = disk;
            this.OnPropertyChanged(nameof(this.Disk));
        }

        private async Task<AppxVolume> GetDisk(string filePath)
        {
            var mgr = await this.volumeManagerFactory.GetProxyFor(SelfElevationLevel.AsInvoker).ConfigureAwait(false);
            var disk = await mgr.GetVolumeForPath(filePath).ConfigureAwait(false);
            if (disk == null)
            {
                return null;
            }

            return disk;
        }

        public AppxVolume Disk { get; private set; }
    }
}
