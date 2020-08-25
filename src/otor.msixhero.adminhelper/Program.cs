using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Diagnostic.Registry;
using Otor.MsixHero.Appx.Packaging.Installation;
using Otor.MsixHero.Appx.Signing;
using Otor.MsixHero.Appx.Volumes;
using Otor.MsixHero.Appx.WindowsVirtualDesktop.AppAttach;
using Otor.MsixHero.Infrastructure.Logging;
using Otor.MsixHero.Infrastructure.Processes.Ipc;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation.Enums;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Lib.Proxy.Diagnostic;
using Otor.MsixHero.Lib.Proxy.Packaging;
using Otor.MsixHero.Lib.Proxy.Signing;
using Otor.MsixHero.Lib.Proxy.Volumes;
using Otor.MsixHero.Lib.Proxy.WindowsVirtualDesktop;

namespace Otor.MsixHero.AdminHelper
{
    public class Program
    {
        private static readonly ILog Logger = LogManager.GetLogger();

        static Program()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;

#if DEBUG
            LogManager.Initialize(MsixHeroLogLevel.Trace);
#else
            LogManager.Initialize(MsixHeroLogLevel.Info);
#endif
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
                if (args.Length > 0 && args[0] == "--selfElevate")
                {
                    Logger.Debug("Preparing to start the pipe server...");

                    ISigningManager signingManager = new SigningManager();
                    IAppAttachManager appAttachManager = new AppAttachManager(signingManager);
                    IAppxVolumeManager appxVolumeManager = new AppxVolumeManager();
                    IRegistryManager registryManager = new RegistryManager();
                    IConfigurationService configurationService = new LocalConfigurationService();
                    IAppxPackageManager appxPackageManager = new AppxPackageManager(registryManager, configurationService);

                    var receivers = new ISelfElevationProxyReceiver[]
                    {
                        new AppAttachManagerProxyReceiver(appAttachManager),
                        new AppxPackageManagerProxyReceiver(appxPackageManager),
                        new AppxVolumeManagerProxyReceiver(appxVolumeManager),
                        new RegistryManagerProxyReceiver(registryManager),
                        new SigningManagerProxyReceiver(signingManager)
                    };

                    var server = new Server(receivers);
                    server.Start().GetAwaiter().GetResult();
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
