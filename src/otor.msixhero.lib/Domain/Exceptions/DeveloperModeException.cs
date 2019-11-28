using System;

namespace otor.msixhero.lib.Domain.Exceptions
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
