using Otor.MsixHero.Lib.Domain.Commands;

namespace Otor.MsixHero.Lib.Proxy.Packaging.Dto
{
    public class AddDto : ProxyObject
    {
        public AddDto()
        {
            this.KillRunningApps = true;
        }

        public AddDto(string filePath)
        {
            this.KillRunningApps = true;
            this.FilePath = filePath;
        }

        public string FilePath { get; set; }

        public bool AllUsers { get; set; }

        public bool AllowDowngrade { get; set; }

        public bool KillRunningApps { get; set; }
    }
}
