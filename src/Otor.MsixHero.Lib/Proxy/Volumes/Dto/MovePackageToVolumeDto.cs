using System;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;
using Otor.MsixHero.Appx.Volumes.Entities;
using Otor.MsixHero.Lib.Domain.Commands;

namespace Otor.MsixHero.Lib.Proxy.Volumes.Dto
{
    public class MovePackageToVolumeDto : ProxyObject
    {
        public MovePackageToVolumeDto()
        {
        }

        public MovePackageToVolumeDto(string volumePath, string packageFullName)
        {
            PackageFullName = packageFullName;
            VolumePackagePath = volumePath;
        }

        public MovePackageToVolumeDto(AppxPackage package, AppxVolume volume)
        {
            if (package == null)
            {
                throw new ArgumentNullException(nameof(package));
            }

            if (volume == null)
            {
                throw new ArgumentNullException(nameof(volume));
            }

            this.PackageFullName = package.FullName;
            this.VolumePackagePath = volume.PackageStorePath;
        }

        public string PackageFullName { get; set; }

        public string VolumePackagePath { get; set; }
    }
}
