using Otor.MsixHero.Ui.Hero.Commands.Base;
using Prism.Events;

namespace Otor.MsixHero.Ui.Hero.Events.Base
{
    public class UiStartedEvent<TCommand> : PubSubEvent<UiStartedPayload<TCommand>> where TCommand : UiCommand
    {
    }
}