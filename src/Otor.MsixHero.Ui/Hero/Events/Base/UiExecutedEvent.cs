using Otor.MsixHero.Ui.Hero.Commands.Base;
using Prism.Events;

namespace Otor.MsixHero.Ui.Hero.Events.Base
{
    public class UiExecutedEvent<TCommand, TResult> : PubSubEvent<UiExecutedPayload<TCommand, TResult>> where TCommand : UiCommand<TResult>
    {
    }

    public class UiExecutedEvent<TCommand> : PubSubEvent<UiExecutedPayload<TCommand>> where TCommand : UiCommand
    {
    }
}