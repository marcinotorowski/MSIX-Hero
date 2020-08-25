using System;

namespace Otor.MsixHero.Infrastructure.Processes.Ipc
{
    public class ForwardedException : Exception
    {
        public ForwardedException(Exception innerException) : base(innerException.Message, innerException)
        {
        }

        public ForwardedException(string message) : base(message)
        {
        }

        public ForwardedException()
        {
        }
    }
}
