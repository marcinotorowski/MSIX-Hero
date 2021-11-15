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

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using NLog;
using NLog.Targets;

namespace Otor.MsixHero.Infrastructure.Logging
{
    public class LogManager
    {
        public static string LogFile { get; private set; }

        public static ILog GetLogger<T>()
        {
            return GetLogger(typeof(T));
        }

        public static ILog GetLogger(Type type)
        {
            return GetLogger(type.FullName);
        }

        public static ILog GetLogger(string type)
        {
            return new NLogWrapper(NLog.LogManager.GetLogger(type));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static ILog GetLogger()
        {
            // ReSharper disable once PossibleNullReferenceException
            var mth = new StackTrace().GetFrame(1).GetMethod();
            if (mth == null || mth.DeclaringType == null)
            {
                throw new InvalidOperationException();
            }

            var cls = mth.DeclaringType.FullName;

            return GetLogger(cls);
        }
        
        public static void Initialize(MsixHeroLogLevel minLogLevel = MsixHeroLogLevel.Info, MsixHeroLogLevel maxLogLevel = MsixHeroLogLevel.Fatal)
        {
            var assembly = Path.GetFileName((Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly()).Location);
            var fileName = $"{assembly}\\{DateTime.Now:yyyyMMdd-hhmmss}_{minLogLevel}.log";
            var targetFileNameInfo = new FileInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MSIX-Hero", "Logs", fileName));

            Initialize(targetFileNameInfo.FullName, minLogLevel, maxLogLevel);
        }

        public static void Initialize(string logFile, MsixHeroLogLevel minLogLevel = MsixHeroLogLevel.Debug, MsixHeroLogLevel maxLogLevel = MsixHeroLogLevel.Fatal)
        {
            if (string.IsNullOrEmpty(logFile))
            {
                throw new ArgumentNullException(nameof(logFile));
            }

            LogFile = logFile;

            var logFileInfo = new FileInfo(logFile);
            if (logFileInfo.Directory?.Exists != true)
            {
                logFileInfo.Directory?.Create();
            }
            else if (!logFileInfo.Exists)
            {
                File.WriteAllText(logFileInfo.FullName, string.Empty);
            }

            var config = new NLog.Config.LoggingConfiguration();

            var fileTarget = new FileTarget
            {
                FileName = logFile,
                Header = GetHeader(),
                Layout = "${longdate}\t${level:uppercase=true}\t${logger}\t${message}\t${exception:format=tostring}"
            };

            config.AddRule(Convert(minLogLevel), Convert(maxLogLevel), fileTarget);

            config.AddRule(LogLevel.Warn, LogLevel.Fatal, new ColoredConsoleTarget
            {
                Layout = "${message}",
                DetectConsoleAvailable = true
            });

            NLog.LogManager.Configuration = config;
        }
        
        private static string GetHeader()
        {
            return "Command line:   '" +Environment.CommandLine + @"'
Date:           '${shortdate}'
Time:           '${time}'
AppDomain:      '${appdomain}'
AssemblyVer:    '${assembly-version}'
WorkingDir:     '${currentdir}'
MachineName:    '${machinename}'
User:           '${environment-user}'
ProcessID:      '${processid}'
ProcessName:    '${processname}'
ThreadID:       '${threadid}'
BaseDir:        '${basedir}'
Architecture:   '${environment:PROCESSOR_ARCHITECTURE}'";
        }

        private static LogLevel Convert(MsixHeroLogLevel level)
        {
            switch (level)
            {
                case MsixHeroLogLevel.Trace:
                    return LogLevel.Trace;
                case MsixHeroLogLevel.Debug:
                    return LogLevel.Debug;
                case MsixHeroLogLevel.Info:
                    return LogLevel.Info;
                case MsixHeroLogLevel.Warn:
                    return LogLevel.Warn;
                case MsixHeroLogLevel.Error:
                    return LogLevel.Error;
                case MsixHeroLogLevel.Fatal:
                    return LogLevel.Fatal;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }
        }
    }
}