using Otor.MsixHero.App.Hero.Commands.Base;
using Otor.MsixHero.Appx.Diagnostic.Logging.Entities;

namespace Otor.MsixHero.App.Hero.Commands.EventViewer
{
    public class SelectLogCommand : UiCommand<Log>
    {
        public SelectLogCommand()
        {
        }

        public SelectLogCommand(Log log)
        {
            this.SelectedLog = log;
        }

        public Log SelectedLog { get; set; }
    }
}
