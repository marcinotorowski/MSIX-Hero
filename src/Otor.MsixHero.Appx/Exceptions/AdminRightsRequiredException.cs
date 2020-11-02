using System;

namespace Otor.MsixHero.Appx.Exceptions
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
