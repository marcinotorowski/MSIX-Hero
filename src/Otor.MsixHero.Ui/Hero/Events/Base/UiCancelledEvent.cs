using Otor.MsixHero.Ui.Hero.Commands.Base;
using Prism.Events;

namespace Otor.MsixHero.Ui.Hero.Events.Base
{
    public class UiCancelledEvent<TCommand> : PubSubEvent<UiCancelledPayload<TCommand>> where TCommand : UiCommand
    {
    }
}