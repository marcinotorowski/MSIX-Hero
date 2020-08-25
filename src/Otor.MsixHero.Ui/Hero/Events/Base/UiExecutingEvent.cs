using Otor.MsixHero.Ui.Hero.Commands.Base;
using Prism.Events;

namespace Otor.MsixHero.Ui.Hero.Events.Base
{
    public class UiExecutingEvent<TCommand> : PubSubEvent<UiExecutingPayload<TCommand>> where TCommand : UiCommand
    {
    }
}