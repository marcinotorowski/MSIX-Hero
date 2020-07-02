using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Commands;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Commanding;
using otor.msixhero.lib.Infrastructure.Configuration;
using otor.msixhero.lib.Infrastructure.Interop;
using otor.msixhero.lib.Infrastructure.Ipc;
using otor.msixhero.lib.Infrastructure.Logging;
using otor.msixhero.lib.Infrastructure.Progress;
using otor.msixhero.lib.Infrastructure.SelfElevation;
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

                    IConfigurationService configurationService = new LocalConfigurationService();
                    IInteractionService interactionService = new SilentInteractionService();
                    IBusyManager busyManager = new BusyManager();
                    IEventAggregator eventAggregator = new EventAggregator();
                    IProcessManager processManager = new ProcessManager();

                    var client = new Client();
                    var factory = new SelfElevationManagerFactory(client, configurationService);

                    var serverBus = new ServerCommandBus(
                        interactionService,
                        client,
                        factory,
                        factory,
                        factory,
                        factory, 
                        factory);
                    
                    var applicationStateManager = new ApplicationStateManager(eventAggregator, serverBus, configurationService);
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
                return InteractionResult.None;
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
                return InteractionResult.None;
            }

            public InteractionResult ShowError(string body, Exception exception,  InteractionResult buttons = InteractionResult.Close)
            {
                return InteractionResult.None;
            }

            public int ShowMessage(string body, IReadOnlyCollection<string> buttons, string title = null, string extendedInfo = null, InteractionResult systemButtons = InteractionResult.None)
            {
                return -1;
            }
        }

        private class Client : IElevatedClient
        {
            public Task<bool> Test(CancellationToken cancellationToken = default)
            {
                return Task.FromResult(true);
            }

            public Task Execute(VoidCommand command, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
            {
                throw new NotImplementedException();
            }

            public Task<TOutput> GetExecuted<TOutput>(CommandWithOutput<TOutput> command, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
            {
                throw new NotImplementedException();
            }
        }
    }
}
