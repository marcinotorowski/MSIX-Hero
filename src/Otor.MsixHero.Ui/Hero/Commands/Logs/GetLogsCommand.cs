using System.Collections.Generic;
using Otor.MsixHero.Appx.Diagnostic.Logging.Entities;
using Otor.MsixHero.Ui.Hero.Commands.Base;

namespace Otor.MsixHero.Ui.Hero.Commands.Logs
{
    public class GetLogsCommand : UiCommand<IList<Log>>
    {
        public GetLogsCommand() : this(250)
        {
        }

        public GetLogsCommand(int count)
        {
            Count = count;
        }

        public int Count { get; }
    }
}
