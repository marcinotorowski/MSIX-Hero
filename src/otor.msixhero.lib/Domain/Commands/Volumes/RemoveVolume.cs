using System.Printing;
using otor.msixhero.lib.Domain.Appx.Volume;

namespace otor.msixhero.lib.Domain.Commands.Volumes
{
    public class RemoveVolume : VoidCommand
    {
        public RemoveVolume()
        {
        }

        public RemoveVolume(string volumeName)
        {
            this.Name = volumeName;
        }

        public RemoveVolume(AppxVolume volume) : this(volume.Name)
        {
        }

        public string Name { get; set; }
    }
}