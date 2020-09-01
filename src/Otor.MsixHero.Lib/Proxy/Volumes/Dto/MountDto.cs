using Otor.MsixHero.Appx.Volumes.Entities;

namespace Otor.MsixHero.Lib.Proxy.Volumes.Dto
{
    public class MountDto : ProxyObject
    {
        public MountDto()
        {
        }

        public MountDto(string volumeName)
        {
            this.Name = volumeName;
        }

        public MountDto(AppxVolume volume) : this(volume.Name)
        {
        }

        public string Name { get; set; }
    }
}