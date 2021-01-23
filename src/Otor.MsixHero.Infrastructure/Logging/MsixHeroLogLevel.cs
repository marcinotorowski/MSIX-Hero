// MSIX Hero
// Copyright (C) 2021 Marcin Otorowski
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

namespace Otor.MsixHero.Infrastructure.Logging
{
    public enum MsixHeroLogLevel
    {
        /// <summary>
        /// Very detailed logs, which may include high-volume information such as protocol payloads. This log level is typically only enabled during development
        /// </summary>
        Trace,

        /// <summary>
        /// Debugging information, less detailed than trace, typically not enabled in production environment.
        /// </summary>
        Debug,

        /// <summary>
        /// Information messages, which are normally enabled in production environment.
        /// </summary>
        Info,

        /// <summary>
        /// Warning messages, typically for non-critical issues, which can be recovered or which are temporary failures.
        /// </summary>
        Warn,

        /// <summary>
        /// Error messages - most of the time these are Exceptions.
        /// </summary>
        Error,

        /// <summary>
        /// Very serious errors!
        /// </summary>
        Fatal
    }
}