// MSIX Hero
// Copyright (C) 2022 Marcin Otorowski
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// Full notice:
// https://github.com/marcinotorowski/msix-hero/blob/develop/LICENSE.md

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
