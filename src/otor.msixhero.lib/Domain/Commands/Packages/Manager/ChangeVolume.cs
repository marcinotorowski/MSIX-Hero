using System;
using otor.msixhero.lib.Domain.Appx.Manifest.Full;
using otor.msixhero.lib.Domain.Appx.Volume;

namespace otor.msixhero.lib.Domain.Commands.Packages.Manager
{
    public class ChangeVolume : VoidCommand
    {
        public ChangeVolume()
        {
        }

        public ChangeVolume(string volumePath, string packageFullName)
        {
            PackageFullName = packageFullName;
            VolumePackagePath = volumePath;
        }

        public ChangeVolume(AppxPackage package, AppxVolume volume)
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
