using System.Collections.Generic;
using Otor.MsixHero.Appx.Volumes.Entities;

namespace Otor.MsixHero.App.Hero.State
{
    public class VolumesState
    {
        public VolumesState()
        {
            this.AllVolumes = new List<AppxVolume>();
            this.SelectedVolumes = new List<AppxVolume>();
        }

        public List<AppxVolume> AllVolumes { get; }

        public List<AppxVolume> SelectedVolumes { get; set; }
        
        public string SearchKey { get; set; }
    }
}