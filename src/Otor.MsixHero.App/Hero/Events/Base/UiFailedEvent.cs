using Otor.MsixHero.App.Hero.Commands.Base;
using Prism.Events;

namespace Otor.MsixHero.App.Hero.Events.Base
{
    public class UiFailedEvent<TCommand> : PubSubEvent<UiFailedPayload<TCommand>> where TCommand : UiCommand
    {
    }
}