using System;
using Otor.MsixHero.App.Hero.Commands.Base;

namespace Otor.MsixHero.App.Hero.Events.Base
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