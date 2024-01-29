// MSIX Hero
// Copyright (C) 2024 Marcin Otorowski
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

namespace Otor.MsixHero.Infrastructure.Logging
{
    public interface ILog
    {
        void Error(Exception exception, string message);
        void Error(Exception exception);
        void Error(string message);
        void Error(string pattern, params object[] parameters);
        void Error(Exception exception, string pattern, params object[] parameters);

        void Fatal(Exception exception, string message);
        void Fatal(Exception exception);
        void Fatal(string message);
        void Fatal(string pattern, params object[] parameters);
        void Fatal(Exception exception, string pattern, params object[] parameters);

        void Info(Exception exception, string message);
        void Info(Exception exception);
        void Info(string message);
        void Info(string pattern, params object[] parameters);
        void Info(Exception exception, string pattern, params object[] parameters);

        void Trace(Exception exception, string message);
        void Trace(Exception exception);
        void Trace(string message);
        void Trace(string pattern, params object[] parameters);
        void Trace(Exception exception, string pattern, params object[] parameters);

        void Debug(Exception exception, string message);
        void Debug(Exception exception);
        void Debug(string message);
        void Debug(string pattern, params object[] parameters);
        void Debug(Exception exception, string pattern, params object[] parameters);

        void Warn(Exception exception, string message);
        void Warn(Exception exception);
        void Warn(string message);
        void Warn(string pattern, params object[] parameters);
        void Warn(Exception exception, string pattern, params object[] parameters);
    }
}
