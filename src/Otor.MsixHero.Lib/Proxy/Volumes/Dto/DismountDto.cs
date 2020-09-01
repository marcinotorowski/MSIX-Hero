using Otor.MsixHero.Appx.Volumes.Entities;

namespace Otor.MsixHero.Lib.Proxy.Volumes.Dto
{
    public class DismountDto : ProxyObject
    {
        public DismountDto()
        {
        }

        public DismountDto(string volumeName)
        {
            this.Name = volumeName;
        }

        public DismountDto(AppxVolume volume) : this(volume.Name)
        {
        }

        public string Name { get; set; }
    }
}