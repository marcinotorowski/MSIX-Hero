using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace otor.msixhero.lib.Infrastructure.Logging
{
    public class LogManager
    {
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
            var assembly = Path.GetFileNameWithoutExtension((Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly()).Location);
            var fileName = $"{DateTime.Now:yyyyMMdd-hhmmss}_{assembly}.log";
            var targetFileNameInfo = new FileInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MSIX-Hero", "Logs", fileName));

            Initialize(targetFileNameInfo.FullName, minLogLevel, maxLogLevel);
        }

        public static void Initialize(string logFile, MsixHeroLogLevel minLogLevel = MsixHeroLogLevel.Debug, MsixHeroLogLevel maxLogLevel = MsixHeroLogLevel.Fatal)
        {
            if (string.IsNullOrEmpty(logFile))
            {
                throw new ArgumentNullException(nameof(logFile));
            }

            var config = new NLog.Config.LoggingConfiguration();

            var logfile = new NLog.Targets.FileTarget
            {
                FileName = logFile,
                Header = GetHeader(),
                Layout = "${longdate}\t${level:uppercase=true}\t${logger}\t${message}\t${exception:format=tostring}"
            };

            config.AddRule(Convert(minLogLevel), Convert(maxLogLevel), logfile);
            config.AddRule(Convert(minLogLevel), Convert(maxLogLevel), new ConsoleTarget());

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