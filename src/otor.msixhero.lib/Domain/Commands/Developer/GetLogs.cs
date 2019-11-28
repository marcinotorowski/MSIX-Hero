using System.Collections.Generic;
using otor.msixhero.lib.Domain.Appx.Logs;

namespace otor.msixhero.lib.Domain.Commands.Developer
{
    public class GetLogs : SelfElevatedCommand<List<Log>>
    {
        public GetLogs()
        {
        }

        public GetLogs(int maxCount)
        {
            MaxCount = maxCount;
        }

        public int MaxCount { get; set; }
    }
}
