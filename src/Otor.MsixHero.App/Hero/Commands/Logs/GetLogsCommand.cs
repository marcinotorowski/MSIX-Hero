using System.Collections.Generic;
using Otor.MsixHero.App.Hero.Commands.Base;
using Otor.MsixHero.Appx.Diagnostic.Logging.Entities;

namespace Otor.MsixHero.App.Hero.Commands.Logs
{
    public class GetLogsCommand : UiCommand<IList<Log>>
    {
        public GetLogsCommand() : this(0)
        {
        }

        public GetLogsCommand(int count)
        {
            Count = count;
        }

        public int Count { get; }
    }
}
