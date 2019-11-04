using System;
using System.Collections.Generic;
using System.Text;

namespace otor.msixhero.lib.Domain
{
    public class DeveloperModeException : Exception
    {
        public DeveloperModeException()
        {
        }

        public DeveloperModeException(string message) : base(message)
        {
        }

        public DeveloperModeException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
