using System;
using System.Collections.Generic;

namespace Otor.MsixHero.Infrastructure.ThirdParty.Exceptions
{
    public class ProcessWrapperException : InvalidOperationException
    {
        public ProcessWrapperException(
            string message, 
            int exitCode, 
            IList<string> standardError,
            IList<string> standardOutput) : base(message)
        {
            this.ExitCode = exitCode;
            this.StandardError = standardError;
            this.StandardOutput = standardOutput;
        }

        public int ExitCode { get; private set; }

        public IList<string> StandardError { get; private set; }

        public IList<string> StandardOutput { get; private set; }
    }
}
