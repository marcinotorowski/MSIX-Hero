using System;
using System.Collections.Generic;
using System.Text;

namespace otor.msixhero.lib.Infrastructure.Ipc
{
    public class ForwardedException : Exception
    {
        public ForwardedException(Exception innerException) : base(innerException.Message, innerException)
        {
        }

        public ForwardedException(string? message) : base(message)
        {
        }

        public ForwardedException()
        {
        }
    }
}
