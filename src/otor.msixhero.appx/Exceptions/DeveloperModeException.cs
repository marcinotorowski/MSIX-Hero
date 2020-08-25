using System;

namespace Otor.MsixHero.Appx.Exceptions
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
