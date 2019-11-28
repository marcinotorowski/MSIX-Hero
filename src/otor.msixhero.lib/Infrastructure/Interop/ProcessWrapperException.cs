using System;
using System.Collections.Generic;
using System.Text;

namespace otor.msixhero.lib.Infrastructure.Interop
{
    public class ProcessWrapperException : InvalidOperationException
    {
        public ProcessWrapperException(string message, int exitCode, IList<string> standardError) : base(message)
        {
            this.ExitCode = exitCode;
            this.StandardError = standardError;
        }

        public int ExitCode { get; private set; }

        public IList<string> StandardError { get; private set; }
    }
}
