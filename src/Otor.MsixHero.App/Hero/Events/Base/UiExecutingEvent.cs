using Otor.MsixHero.App.Hero.Commands.Base;
using Prism.Events;

namespace Otor.MsixHero.App.Hero.Events.Base
{
    public class UiExecutingEvent<TCommand> : PubSubEvent<UiExecutingPayload<TCommand>> where TCommand : UiCommand
    {
    }
}