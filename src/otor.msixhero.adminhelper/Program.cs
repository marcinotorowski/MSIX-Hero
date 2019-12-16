using System;
using otor.msixhero.lib.BusinessLayer.Appx;
using otor.msixhero.lib.BusinessLayer.Appx.Signing;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Commanding;
using otor.msixhero.lib.Infrastructure.Configuration;
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
#if DEBUG
            LogManager.Initialize(MsixHeroLogLevel.Debug);
#else
            LogManager.Initialize(MsixHeroLogLevel.Info);
#endif
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
                    IAppxPackageManagerFactory packageManagerFactory = new AppxPackageFactory(new AppxSigningManager());
                    IBusyManager busyManager = new BusyManager();
                    IEventAggregator eventAggregator = new EventAggregator();

                    var commandExecutor = new CommandExecutor(packageManagerFactory, interactionService, busyManager);
                    var applicationStateManager = new ApplicationStateManager(eventAggregator, commandExecutor, configurationService);
                    var server = new Server(applicationStateManager);

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

        private class AppxPackageFactory : IAppxPackageManagerFactory
        {
            private CurrentUserAppxPackageManager factory;

            public AppxPackageFactory(IAppxSigningManager signingManager)
            {
                this.factory = new CurrentUserAppxPackageManager(signingManager);
            }

            public IAppxPackageManager GetLocal()
            {
                return this.factory;
            }

            public IAppxPackageManager GetRemote()
            {
                return this.GetLocal();
            }
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
        }
    }
}
