using Otor.MsixHero.Appx.Diagnostic.Logging.Enums;
using Otor.MsixHero.Ui.Hero.Commands.Base;

namespace Otor.MsixHero.Ui.Hero.Commands.Logs
{
    public class OpenEventViewerCommand : UiCommand
    {
        public OpenEventViewerCommand(EventLogCategory type)
        {
            this.Type = type;
        }

        public EventLogCategory Type { get; private set; }
    }
}