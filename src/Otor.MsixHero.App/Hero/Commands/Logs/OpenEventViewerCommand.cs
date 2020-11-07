using Otor.MsixHero.App.Hero.Commands.Base;
using Otor.MsixHero.Appx.Diagnostic.Logging.Enums;

namespace Otor.MsixHero.App.Hero.Commands.Logs
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