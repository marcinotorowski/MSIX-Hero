using Otor.MsixHero.App.Hero.Commands.Base;

namespace Otor.MsixHero.App.Hero.Events.Base
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