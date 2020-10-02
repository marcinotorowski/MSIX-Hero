using System;
using Otor.MsixHero.Ui.Hero.Commands.Base;

namespace Otor.MsixHero.Ui.Hero.Events.Base
{
    public class UiFailedPayload<TCommand> : UiPayload where TCommand : UiCommand
    {
        public UiFailedPayload(object sender, TCommand command, Exception innerException = null) : base(sender)
        {
            this.InnerException = innerException;
            this.Command = command;
        }

        public TCommand Command { get; }

        public Exception InnerException { get; }
    }
}