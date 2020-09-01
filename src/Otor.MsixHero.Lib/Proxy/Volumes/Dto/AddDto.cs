using Otor.MsixHero.Appx.Volumes.Entities;

namespace Otor.MsixHero.Lib.Proxy.Volumes.Dto
{
    public class AddDto : ProxyObject<AppxVolume>
    {
        public AddDto()
        {
        }

        public AddDto(string drivePath)
        {
            DrivePath = drivePath;
        }

        public string DrivePath { get; set; }
    }
}
