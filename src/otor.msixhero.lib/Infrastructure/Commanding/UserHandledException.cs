using System;

namespace otor.msixhero.lib.Infrastructure.Commanding
{
    public class UserHandledException : Exception
    {
        public UserHandledException()
        {
        }

        public UserHandledException(string message) : base(message)
        {
        }

        public UserHandledException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public UserHandledException(Exception innerException) : base("An exception handled by the user.", innerException)
        {
        }
    }
}
