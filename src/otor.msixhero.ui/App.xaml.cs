using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using CommonServiceLocator;
using otor.msixhero.lib.BusinessLayer.Appx;
using otor.msixhero.lib.BusinessLayer.Appx.AppAttach;
using otor.msixhero.lib.BusinessLayer.Appx.Builder;
using otor.msixhero.lib.BusinessLayer.Appx.Packer;
using otor.msixhero.lib.BusinessLayer.Appx.Signing;
using otor.msixhero.lib.BusinessLayer.Appx.VolumeManager;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.BusinessLayer.SystemState.Services;
using otor.msixhero.lib.BusinessLayer.SystemState.ThirdParty;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Commanding;
using otor.msixhero.lib.Infrastructure.Configuration;
using otor.msixhero.lib.Infrastructure.Interop;
using otor.msixhero.lib.Infrastructure.Ipc;
using otor.msixhero.lib.Infrastructure.Logging;
using otor.msixhero.lib.Infrastructure.Update;
using otor.msixhero.ui.Modules.Common;
using otor.msixhero.ui.Modules.Dialogs;
using otor.msixhero.ui.Modules.Main;
using otor.msixhero.ui.Modules.PackageList;
using otor.msixhero.ui.Modules.Settings;
using otor.msixhero.ui.Modules.SystemStatus;
using otor.msixhero.ui.Modules.VolumeManager;
using otor.msixhero.ui.Services;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Services.Dialogs;

namespace otor.msixhero.ui
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger();
        private string[] arguments;

        static App()
        {
#if DEBUG
            //LogManager.Initialize(MsixHeroLogLevel.Debug);
            LogManager.Initialize(MsixHeroLogLevel.Info);
#else
            LogManager.Initialize(MsixHeroLogLevel.Info);
#endif
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            Dispatcher.CurrentDispatcher.UnhandledException += CurrentDispatcherOnUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;
        }

        private static void CurrentDispatcherOnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (e.Exception is OperationCanceledException)
            {
                e.Handled = true;
                return;
            }

            Logger.Warn(e.Exception);
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


        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IInteractionService, InteractionService>();
            containerRegistry.RegisterSingleton<IAppxSigningManager, AppxSigningManager>();
            containerRegistry.RegisterSingleton<IAppxPacker, AppxPacker>();
            containerRegistry.RegisterSingleton<IAppAttach, AppAttach>();
            containerRegistry.RegisterSingleton<IAppxContentBuilder, AppxContentBuilder>();
            containerRegistry.RegisterSingleton<IElevatedClient, Client>();
            containerRegistry.RegisterSingleton<IBusyManager, BusyManager>();
            containerRegistry.RegisterSingleton<IConfigurationService, LocalConfigurationService>();
            containerRegistry.RegisterSingleton<IWritableApplicationStateManager, ApplicationStateManager>();
            containerRegistry.RegisterSingleton<IApplicationStateManager, ApplicationStateManager>();
            containerRegistry.RegisterSingleton<ICommandExecutor, CommandExecutor>();
            containerRegistry.RegisterSingleton<ICommandExecutor, CommandExecutor>();
            containerRegistry.RegisterSingleton<IUpdateChecker, HttpUpdateChecker>();
            containerRegistry.RegisterSingleton<IAppxVolumeManager, AppxVolumeManager>();
            containerRegistry.RegisterSingleton<IAppxPackageManager, AppxPackageManager>();
            containerRegistry.RegisterSingleton<IProcessManager, ProcessManager>();
            containerRegistry.Register<IThirdPartyAppProvider, ThirdPartyAppProvider>();
            containerRegistry.Register<IServiceRecommendationAdvisor, ServiceRecommendationAdvisor>();
        }
        
        protected override void OnStartup(StartupEventArgs startupEventArgs)
        {
            this.arguments = startupEventArgs.Args;
            base.OnStartup(startupEventArgs);
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            if (this.arguments.Any())
            {
                var dlg = ServiceLocator.Current.GetInstance<IDialogService>();
                var handler = new ExplorerHandler(dlg, true);
                handler.Handle(this.arguments.First());
            }
        }

        protected override Window CreateShell()
        {
            if (this.arguments.Any())
            {
                return null;
            }

            var shell = ServiceLocator.Current.GetInstance<MainWindow>();
            return shell;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (this.Container.Resolve<IApplicationStateManager>() is IDisposable appStateManager)
            {
                appStateManager.Dispose();
            }

            var processManager = ServiceLocator.Current.GetInstance<IProcessManager>();
            processManager.Dispose();
            base.OnExit(e);
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            moduleCatalog.AddModule<MainModule>();
            moduleCatalog.AddModule<SettingsModule>();
            moduleCatalog.AddModule<PackageListModule>(); 
            moduleCatalog.AddModule<VolumeManagerModule>();
            moduleCatalog.AddModule<SystemStatusModule>();
            moduleCatalog.AddModule<DialogsModule>();
            moduleCatalog.AddModule<CommonModule>();
        }
        
        protected override void ConfigureViewModelLocator()
        {
            base.ConfigureViewModelLocator();
        }

        public void Dispose()
        {
            var processManager = ServiceLocator.Current.GetInstance<IProcessManager>();
            processManager.Dispose();
        }
    }
}
