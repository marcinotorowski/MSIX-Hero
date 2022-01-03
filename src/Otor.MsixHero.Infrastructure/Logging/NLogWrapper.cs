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
using NLog;

namespace Otor.MsixHero.Infrastructure.Logging
{
    internal class NLogWrapper : ILog
    {
        private readonly Logger logger;

        public NLogWrapper(Logger logger)
        {
            this.logger = logger;
        }

        public void Fatal(Exception exception, string message)
        {
            this.logger.Fatal(exception, message);
        }

        public void Fatal(Exception exception)
        {
            this.logger.Fatal(exception);
        }

        public void Fatal(string message)
        {
            this.logger.Fatal(message);
        }

        public void Fatal(string pattern, params object[] parameters)
        {
            this.logger.Fatal(pattern, parameters);
        }

        public void Fatal(Exception exception, string pattern, params object[] parameters)
        {
            this.logger.Fatal(exception, pattern, parameters);
        }

        public void Error(Exception exception, string message)
        {
            this.logger.Error(exception, message);
        }

        public void Error(Exception exception)
        {
            this.logger.Error(exception);
        }

        public void Error(string message)
        {
            this.logger.Error(message);
        }

        public void Error(string pattern, params object[] parameters)
        {
            this.logger.Error(pattern, parameters);
        }

        public void Error(Exception exception, string pattern, params object[] parameters)
        {
            this.logger.Error(exception, pattern, parameters);
        }

        public void Info(Exception exception, string message)
        {
            this.logger.Info(exception, message);
        }

        public void Info(Exception exception)
        {
            this.logger.Info(exception);
        }

        public void Info(string message)
        {
            this.logger.Info(message);
        }

        public void Info(string pattern, params object[] parameters)
        {
            this.logger.Info(pattern, parameters);
        }

        public void Info(Exception exception, string pattern, params object[] parameters)
        {
            this.logger.Info(exception, pattern, parameters);
        }

        public void Warn(Exception exception, string message)
        {
            this.logger.Warn(exception, message);
        }

        public void Warn(Exception exception)
        {
            this.logger.Warn(exception);
        }

        public void Warn(string message)
        {
            this.logger.Warn(message);
        }

        public void Warn(string pattern, params object[] parameters)
        {
            this.logger.Warn(pattern, parameters);
        }

        public void Warn(Exception exception, string pattern, params object[] parameters)
        {
            this.logger.Warn(exception, pattern, parameters);
        }

        public void Trace(Exception exception, string message)
        {
            this.logger.Trace(exception, message);
        }

        public void Trace(Exception exception)
        {
            this.logger.Trace(exception);
        }

        public void Trace(string message)
        {
            this.logger.Trace(message);
        }

        public void Trace(string pattern, params object[] parameters)
        {
            this.logger.Trace(pattern, parameters);
        }

        public void Trace(Exception exception, string pattern, params object[] parameters)
        {
            this.logger.Trace(exception, pattern, parameters);
        }

        public void Debug(Exception exception, string message)
        {
            this.logger.Debug(exception, message);
        }

        public void Debug(Exception exception)
        {
            this.logger.Debug(exception);
        }

        public void Debug(string message)
        {
            this.logger.Debug(message);
        }

        public void Debug(string pattern, params object[] parameters)
        {
            this.logger.Debug(pattern, parameters);
        }

        public void Debug(Exception exception, string pattern, params object[] parameters)
        {
            this.logger.Debug(exception, pattern, parameters);
        }
    }
}