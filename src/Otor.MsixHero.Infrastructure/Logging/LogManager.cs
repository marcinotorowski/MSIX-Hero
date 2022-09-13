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
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Dapplo.Log;
using NLog;
using NLog.Targets;

namespace Otor.MsixHero.Infrastructure.Logging
{
    public class LogManager
    {
        public static string LogFile { get; private set; }

        public static string GetActualLogFile()
        {
            if (LogFile == null || File.Exists(LogFile))
            {
                return LogFile;
            }

            // If we are here then it means we are running with identity from an MSIX and that local app data (where the logs are stored by default)
            // is redirected. So let's make sure we return the correct file.
            var familyName = GetAppxFamilyNameIfRunningAsUwp();
            if (string.IsNullOrEmpty(familyName))
            {
                return LogFile;
            }
            
            var virtualFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData).Trim('\\') + "\\";
            if (LogFile.StartsWith(virtualFolder, StringComparison.OrdinalIgnoreCase))
            {
                var redirected = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Packages", familyName, "LocalCache", "Local", LogFile.Remove(0, virtualFolder.Length));
                if (File.Exists(redirected))
                {
                    return redirected;
                }
            }

            virtualFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Trim('\\') + "\\";
            if (LogFile.StartsWith(virtualFolder, StringComparison.OrdinalIgnoreCase))
            {
                var redirected = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Packages", familyName, "LocalCache", "Roaming", LogFile.Remove(0, virtualFolder.Length));
                if (File.Exists(redirected))
                {
                    return redirected;
                }
            }

            return LogFile;
        }
        
        public static void Initialize(MsixHeroLogLevel minLogLevel = MsixHeroLogLevel.Info)
        {
            var assembly = Path.GetFileName((Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly()).Location);

            // Rewrite cryptic assembly names into something meaningful
            switch (Path.GetFileNameWithoutExtension(assembly).ToLowerInvariant())
            {
                case "msixhero":
                    assembly = "app";
                    break;
                case "msixherocli":
                    assembly = "cli";
                    break;
                case "msixhero-uac":
                    assembly = "uac";
                    break;
            }

            var fileName = $"{assembly}\\{DateTime.Now:yyyyMMdd-HHmmss}_{minLogLevel}.log";
            var targetFileNameInfo = new FileInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MSIX-Hero", "Logs", fileName));

            Initialize(targetFileNameInfo.FullName, minLogLevel);
        }

        public static void Initialize(string logFile, MsixHeroLogLevel minLogLevel = MsixHeroLogLevel.Debug)
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
                Header = GetHeader()
            };

            config.AddRuleForAllLevels(fileTarget);
            config.AddRule(LogLevel.Warn, LogLevel.Fatal, new ColoredConsoleTarget
            {
                Layout = "${message}",
                DetectConsoleAvailable = true
            });

            NLog.LogManager.Configuration = config;

            LogSettings.RegisterDefaultLogger<NLogLogger>();
            LogSettings.DefaultLogger.LogLevel = Convert(minLogLevel);
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
Culture:        '${culture}'
UI culture:     '${UIculture}'
User:           '${environment-user}'
ProcessID:      '${processid}'
ProcessName:    '${processname}'
ThreadID:       '${threadid}'
BaseDir:        '${basedir}'
Architecture:   '${environment:PROCESSOR_ARCHITECTURE}'".Replace("${Culture}", CultureInfo.CurrentCulture.Name).Replace("${UIculture}", CultureInfo.CurrentUICulture.Name);
        }

        private static LogLevels Convert(MsixHeroLogLevel level)
        {
            switch (level)
            {
                case MsixHeroLogLevel.Trace:
                    return LogLevels.Verbose;
                case MsixHeroLogLevel.Debug:
                    return LogLevels.Debug;
                case MsixHeroLogLevel.Info:
                    return LogLevels.Info;
                case MsixHeroLogLevel.Warn:
                    return LogLevels.Warn;
                case MsixHeroLogLevel.Error:
                    return LogLevels.Error;
                case MsixHeroLogLevel.Fatal:
                    return LogLevels.Fatal;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int GetCurrentPackageFullName(
            ref int packageFullNameLength,
            StringBuilder packageFullName);

        private static string GetAppxFamilyNameIfRunningAsUwp()
        {
            var packageFullNameLength = 0;
            var packageFullName = new StringBuilder(0);
            if (GetCurrentPackageFullName(ref packageFullNameLength, packageFullName) == 15700)
            {
                return null;
            }

            packageFullName = new StringBuilder(packageFullNameLength);
            if (GetCurrentPackageFullName(ref packageFullNameLength, packageFullName) == 15700)
            {
                return null;
            }

            var fullName = packageFullName.ToString().Split('_');

            var familyName = fullName[0] + "_" + fullName[2];
            return familyName;
        }
    }
}