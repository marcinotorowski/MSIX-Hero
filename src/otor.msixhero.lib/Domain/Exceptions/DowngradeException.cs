using System;

namespace otor.msixhero.lib.Domain.Exceptions
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
