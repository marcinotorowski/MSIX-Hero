using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Appx;
using otor.msixhero.lib.BusinessLayer.Appx.AppAttach;
using otor.msixhero.lib.BusinessLayer.Appx.Signing;
using otor.msixhero.lib.BusinessLayer.Appx.VolumeManager;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Commanding;
using otor.msixhero.lib.Infrastructure.Configuration;
using otor.msixhero.lib.Infrastructure.Interop;
using otor.msixhero.lib.Infrastructure.Ipc;
using otor.msixhero.lib.Infrastructure.Logging;
using Prism.Events;

namespace otor.msixhero.adminhelper
{
    public class Program
    {
        private static readonly ILog Logger = LogManager.GetLogger();

        static Program()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;

#if DEBUG
            LogManager.Initialize(MsixHeroLogLevel.Debug);
#else
            LogManager.Initialize(MsixHeroLogLevel.Info);
#endif
        }

        private static void TaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
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

                    IConfigurationService configurationService = new LocalConfigurationService();
                    IInteractionService interactionService = new SilentInteractionService();
                    IBusyManager busyManager = new BusyManager();
                    IEventAggregator eventAggregator = new EventAggregator();
                    IAppAttach appAttach = new AppAttach();
                    IAppxVolumeManager volumeManager = new AppxVolumeManager();
                    IProcessManager processManager = new ProcessManager();

                    var commandExecutor = new CommandExecutor(new AppxPackageManager(new AppxSigningManager()), volumeManager, interactionService, new Client(processManager), appAttach);
                    var applicationStateManager = new ApplicationStateManager(eventAggregator, commandExecutor, configurationService);
                    var server = new Server(applicationStateManager);

                    server.Start(busyManager: busyManager).GetAwaiter().GetResult();

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

        private class SilentInteractionService : IInteractionService
        {
            public InteractionResult Confirm(string body, string title = null, InteractionType type = InteractionType.Asterisk, InteractionButton buttons = InteractionButton.OK)
            {
                return InteractionResult.OK;
            }

            public bool SelectFile(string initialFile, string filterString, out string selectedFile)
            {
                selectedFile = null;
                return false;
            }

            public bool SelectFile(string filterString, out string selectedFile)
            {
                selectedFile = null;
                return false;
            }

            public bool SaveFile(string initialFile, string filterString, out string selectedFile)
            {
                selectedFile = null;
                return false;
            }

            public bool SaveFile(string filterString, out string selectedFile)
            {
                selectedFile = null;
                return false;
            }

            public bool SelectFiles(string initialFile, string filterString, out string[] selectedFiles)
            {
                selectedFiles = null;
                return false;
            }

            public bool SelectFiles(string filterString, out string[] selectedFiles)
            {
                selectedFiles = null;
                return false;
            }

            public bool SelectFile(out string selectedFile)
            {
                selectedFile = null;
                return false;
            }

            public bool SaveFile(out string selectedFile)
            {
                selectedFile = null;
                return false;
            }

            public bool SelectFolder(string initialFile, out string selectedFolder)
            {
                selectedFolder = null;
                return false;
            }

            public bool SelectFolder(out string selectedFolder)
            {
                selectedFolder = null;
                return false;
            }

            public InteractionResult ShowError(string body, InteractionResult buttons = InteractionResult.Close, string title = null, string extendedInfo = null)
            {
                return InteractionResult.OK;
            }

            public InteractionResult ShowError(string body, Exception exception,  InteractionResult buttons = InteractionResult.Close)
            {
                return InteractionResult.OK;
            }

            public int ShowMessage(string body, IReadOnlyCollection<string> buttons, string title = null, string extendedInfo = null)
            {
                return -1;
            }
        }
    }
}
