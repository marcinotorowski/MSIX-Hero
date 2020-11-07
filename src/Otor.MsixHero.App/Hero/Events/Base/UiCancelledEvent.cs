using Otor.MsixHero.App.Hero.Commands.Base;
using Prism.Events;

namespace Otor.MsixHero.App.Hero.Events.Base
{
    public class UiCancelledEvent<TCommand> : PubSubEvent<UiCancelledPayload<TCommand>> where TCommand : UiCommand
    {
    }
}