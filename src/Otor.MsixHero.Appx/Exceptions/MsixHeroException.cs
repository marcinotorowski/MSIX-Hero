using System;

namespace Otor.MsixHero.Appx.Exceptions
{
    public class MsixHeroException : ApplicationException
    {
        public MsixHeroException()
        {
        }

        public MsixHeroException(string message) : base(message)
        {
        }

        public MsixHeroException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
