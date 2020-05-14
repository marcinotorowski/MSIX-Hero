﻿using otor.msixhero.lib.Domain.Appx.Volume;

namespace otor.msixhero.lib.Domain.Commands.Volumes
{
    public class DismountVolume : VoidCommand
    {
        public DismountVolume()
        {
        }

        public DismountVolume(string volumeName)
        {
            this.Name = volumeName;
        }

        public DismountVolume(AppxVolume volume) : this(volume.Name)
        {
        }

        public string Name { get; set; }
    }
}