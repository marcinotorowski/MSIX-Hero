using System;

namespace otor.msixhero.lib.Infrastructure.Interop
{
    public class SdkException : InvalidOperationException
    {
        public SdkException(string message, int exitCode) : base(message)
        {
            this.ExitCode = exitCode;
        }

        public int ExitCode { get; }
    }
}