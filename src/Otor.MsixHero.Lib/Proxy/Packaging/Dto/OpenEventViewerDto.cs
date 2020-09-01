using Otor.MsixHero.Appx.Diagnostic.Logging.Enums;

namespace Otor.MsixHero.Lib.Proxy.Packaging.Dto
{
    public class OpenEventViewerDto : ProxyObject
    {
        public OpenEventViewerDto()
        {
        }

        public EventLogCategory Type { get; set; }
    }
}