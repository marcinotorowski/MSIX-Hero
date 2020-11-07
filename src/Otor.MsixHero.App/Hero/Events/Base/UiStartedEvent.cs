using Otor.MsixHero.App.Hero.Commands.Base;
using Prism.Events;

namespace Otor.MsixHero.App.Hero.Events.Base
{
    public class UiStartedEvent<TCommand> : PubSubEvent<UiStartedPayload<TCommand>> where TCommand : UiCommand
    {
    }
}