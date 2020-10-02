using Otor.MsixHero.Ui.Hero.Commands.Base;

namespace Otor.MsixHero.Ui.Hero.Events.Base
{
    public class UiStartedPayload<TCommand> : UiPayload where TCommand : UiCommand
    {
        public UiStartedPayload(object sender, TCommand command) : base(sender)
        {
            this.Command = command;
        }

        public TCommand Command { get; }
    }
}