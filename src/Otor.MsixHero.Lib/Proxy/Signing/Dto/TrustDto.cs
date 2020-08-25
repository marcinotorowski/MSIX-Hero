using Otor.MsixHero.Lib.Domain.Commands;

namespace Otor.MsixHero.Lib.Proxy.Signing.Dto
{
    public class TrustDto : ProxyObject
    {
        public TrustDto(string filePath)
        {
            this.FilePath = filePath;
        }

        public TrustDto()
        {
        }

        public string FilePath { get; set; }
    }
}
