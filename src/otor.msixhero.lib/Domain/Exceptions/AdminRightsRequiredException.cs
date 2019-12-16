using System;
using System.Runtime.Serialization;

namespace otor.msixhero.lib.Domain.Exceptions
{
    public class AdminRightsRequiredException : InvalidOperationException
    {
        public AdminRightsRequiredException()
        {
        }

        public AdminRightsRequiredException(string message) : base(message)
        {
        }

        public AdminRightsRequiredException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
