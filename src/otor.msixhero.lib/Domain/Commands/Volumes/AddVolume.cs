using System;
using System.Text;
using otor.msixhero.lib.Domain.Appx.Volume;

namespace otor.msixhero.lib.Domain.Commands.Volumes
{
    public class AddVolume : SelfElevatedCommand<AppxVolume>
    {
        public AddVolume()
        {
        }

        public AddVolume(string drivePath)
        {
            DrivePath = drivePath;
        }

        public string DrivePath { get; set; }

        public override bool RequiresElevation { get; } = true;
    }
}
