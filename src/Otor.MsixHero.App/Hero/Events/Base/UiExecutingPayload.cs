using Otor.MsixHero.App.Hero.Commands.Base;

namespace Otor.MsixHero.App.Hero.Events.Base
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