using Otor.MsixHero.Ui.Hero.Commands.Base;

namespace Otor.MsixHero.Ui.Hero.Events.Base
{
    public class UiExecutedPayload<TCommand, TResult> : UiPayload where TCommand : UiCommand<TResult>
    {
        public UiExecutedPayload(object sender, TCommand command, TResult result) : base(sender)
        {
            Result = result;
            this.Command = command;
        }

        public TCommand Command { get; }

        public TResult Result { get; }
    }

    public class UiExecutedPayload<TCommand> : UiPayload where TCommand : UiCommand
    {
        public UiExecutedPayload(object sender, TCommand command) : base(sender)
        {
            this.Command = command;
        }

        public TCommand Command { get; }
    }
}