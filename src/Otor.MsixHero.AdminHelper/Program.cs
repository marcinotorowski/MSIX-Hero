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
using System.Threading.Tasks;
using Dapplo.Log;
using Otor.MsixHero.Appx.Diagnostic.Logging;
using Otor.MsixHero.Appx.Diagnostic.Registry;
using Otor.MsixHero.Appx.Packaging.Installation;
using Otor.MsixHero.Appx.Signing;
using Otor.MsixHero.Appx.Signing.TimeStamping;
using Otor.MsixHero.Appx.Volumes;
using Otor.MsixHero.Appx.WindowsVirtualDesktop.AppAttach;
using Otor.MsixHero.Elevation;
using Otor.MsixHero.Infrastructure.Helpers;
using Otor.MsixHero.Infrastructure.Logging;
using Otor.MsixHero.Infrastructure.Services;

namespace Otor.MsixHero.AdminHelper
{
    public class Program
    {
        private static readonly ILog Logger = LogManager.GetLogger();

        static Program()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;
            
            var logLevel = MsixHeroLogLevel.Info;

            ExceptionGuard.Guard(() =>
            {
                var service = new LocalConfigurationService();
                var config = service.GetCurrentConfiguration();

                logLevel = config.VerboseLogging ? MsixHeroLogLevel.Trace : MsixHeroLogLevel.Info;
            });

            LogManager.Initialize(logLevel);
        }

        private static void TaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            // ReSharper disable once PossibleNullReferenceException
            var ex = e.Exception.GetBaseException();

            if (ex is OperationCanceledException)
            {
                e.SetObserved();
                return;
            }

            Logger.Warn(e.Exception);
        }
        
        private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                Logger.Fatal(ex);
            }
            else
            {
                Logger.Fatal($"Unhandled exception {e.ExceptionObject}");
            }
        }


        public static void Main(string[] args)
        {
            try
            {
                LogSettings.RegisterDefaultLogger<Dapplo.Log.Loggers.ColorConsoleLogger>();
                LogSettings.DefaultLogger.LogLevel = LogLevels.Verbose;

                if (args.Length > 0 && args[0] == "--selfElevate")
                {
                    Logger.Debug("Preparing to start the pipe server...");

                    IConfigurationService configurationService = new LocalConfigurationService();
                    var signingManager = new SigningManager(MsixHeroGistTimeStampFeed.CreateCached());
                    var appAttachManager = new AppAttachManager(signingManager, configurationService);
                    var registryManager = new RegistryManager();
                    var appxPackageQuery = new AppxPackageQuery(registryManager, configurationService);
                    
                    var server = new SimpleElevationServer();
                    server.RegisterProxy<ISigningManager, SigningManager>(signingManager);
                    server.RegisterProxy<IAppAttachManager, AppAttachManager>(appAttachManager);
                    server.RegisterProxy<IRegistryManager, RegistryManager>(registryManager);
                    server.RegisterProxy<IAppxVolumeManager, AppxVolumeManager>();
                    server.RegisterProxy<IAppxPackageInstaller, AppxPackageInstaller>();
                    server.RegisterProxy<IAppxLogManager, AppxLogManager>();
                    server.RegisterProxy<IAppxPackageRunner, AppxPackageRunner>();
                    server.RegisterProxy<IAppxPackageManager, AppxPackageManager>();
                    server.RegisterProxy<IAppxPackageQuery, AppxPackageQuery>(appxPackageQuery);
                    server.StartAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                    Console.ReadKey();
                }
                else
                {
                    Logger.Fatal("Unsupported command line arguments, terminating...");
                    Environment.ExitCode = 1;
                }
            }
            catch (AggregateException e)
            {
                Logger.Fatal(e.GetBaseException(), "Fatal exception, the program will be closed.");
            }
            catch (Exception e)
            {
                Logger.Fatal(e, "Fatal exception, the program will be closed.");
            }

            Logger.Info("Waiting for the user to press a key...");
            Console.ReadKey();
        }
    }
}
