using Otor.MsixHero.Appx.Volumes.Entities;

namespace Otor.MsixHero.Lib.Proxy.Volumes.Dto
{
    public class DeleteDto : ProxyObject
    {
        public DeleteDto()
        {
        }

        public DeleteDto(string volumeName)
        {
            this.Name = volumeName;
        }

        public DeleteDto(AppxVolume volume) : this(volume.Name)
        {
        }

        public string Name { get; set; }
    }
}