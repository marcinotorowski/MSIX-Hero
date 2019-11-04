using System;
using System.Collections.Generic;
using System.Text;
using otor.msixhero.lib.BusinessLayer.Models.Logs;

namespace otor.msixhero.lib.BusinessLayer.Commands.Developer
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
