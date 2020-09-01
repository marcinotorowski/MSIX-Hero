using System.Collections.Generic;
using Otor.MsixHero.Appx.Diagnostic.Logging.Entities;

namespace Otor.MsixHero.Lib.Proxy.Packaging.Dto
{
    public class GetLogsDto : ProxyObject<List<Log>>
    {
        public GetLogsDto()
        {
        }

        public GetLogsDto(int maxCount)
        {
            MaxCount = maxCount;
        }

        public int MaxCount { get; set; }
    }
}
