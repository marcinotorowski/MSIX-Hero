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

using Dapplo.Log;
using NLog;

namespace Otor.MsixHero.Infrastructure.Logging
{
    public class NLogLogger : AbstractLogger
    {
        /// <inheritdoc />
        public override void Write(LogInfo logInfo, string messageTemplate, params object[] logParameters)
        {
            NLog.LogManager.GetLogger(logInfo.Source.Source).Log(Convert(logInfo.LogLevel), messageTemplate?.TrimEnd(), logParameters);
        }

        private static LogLevel Convert(LogLevels logLevel) => logLevel switch
        {
            LogLevels.Info => NLog.LogLevel.Info,
            LogLevels.Warn => NLog.LogLevel.Warn,
            LogLevels.Error => NLog.LogLevel.Error,
            LogLevels.Fatal => NLog.LogLevel.Fatal,
            LogLevels.Verbose => NLog.LogLevel.Trace,
            _ => NLog.LogLevel.Debug,
        };
    }
}