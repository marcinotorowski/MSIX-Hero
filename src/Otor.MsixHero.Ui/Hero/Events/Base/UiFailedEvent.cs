using Otor.MsixHero.Ui.Hero.Commands.Base;
using Prism.Events;

namespace Otor.MsixHero.Ui.Hero.Events.Base
{
    public class UiFailedEvent<TCommand> : PubSubEvent<UiFailedPayload<TCommand>> where TCommand : UiCommand
    {
    }
}