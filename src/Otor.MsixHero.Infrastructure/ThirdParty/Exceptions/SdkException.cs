using System;

namespace Otor.MsixHero.Infrastructure.ThirdParty.Exceptions
{
    public class SdkException : InvalidOperationException
    {
        public SdkException(string message, int exitCode) : base(message)
        {
            this.ExitCode = exitCode;
        }

        public SdkException(string message, int exitCode, Exception baseException) : base(message, baseException)
        {
            this.ExitCode = exitCode;
        }

        public int ExitCode { get; }
    }
}