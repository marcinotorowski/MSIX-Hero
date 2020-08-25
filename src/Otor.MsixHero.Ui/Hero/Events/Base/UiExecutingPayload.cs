using Otor.MsixHero.Ui.Hero.Commands.Base;

namespace Otor.MsixHero.Ui.Hero.Events.Base
{
    public class UiExecutingPayload<TCommand> : UiPayload where TCommand : UiCommand
    {
        public UiExecutingPayload(object sender, TCommand command) : base(sender)
        {
            this.Command = command;
        }

        public TCommand Command { get; }

        public bool Cancel { get; set; }
    }
}