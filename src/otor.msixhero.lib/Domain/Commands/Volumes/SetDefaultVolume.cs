using otor.msixhero.lib.Domain.Appx.Volume;

namespace otor.msixhero.lib.Domain.Commands.Volumes
{
    public class SetDefaultVolume : VoidCommand
    {
        public SetDefaultVolume()
        {
        }

        public SetDefaultVolume(string drivePath)
        {
            this.DrivePath = drivePath;
        }

        public SetDefaultVolume(AppxVolume volume) : this(volume.PackageStorePath)
        {
        }

        public string DrivePath { get; set; }
    }
}