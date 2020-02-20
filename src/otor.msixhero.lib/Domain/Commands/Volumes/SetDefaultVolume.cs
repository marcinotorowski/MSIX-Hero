using otor.msixhero.lib.Domain.Appx.Volume;

namespace otor.msixhero.lib.Domain.Commands.Volumes
{
    public class SetDefaultVolume : SelfElevatedCommand
    {
        public SetDefaultVolume()
        {
        }

        public SetDefaultVolume(string volumeName)
        {
            this.Name = volumeName;
        }

        public SetDefaultVolume(AppxVolume volume) : this(volume.Name)
        {
        }

        public string Name { get; set; }

        public override bool RequiresElevation { get; } = true;
    }
}