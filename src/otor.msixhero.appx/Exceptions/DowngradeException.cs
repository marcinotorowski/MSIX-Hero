using System;

namespace Otor.MsixHero.Appx.Exceptions
{
    public class DowngradeException : Exception
    {
        public DowngradeException()
        {
        }

        public DowngradeException(string message) : base(message)
        {
        }
    }
}
