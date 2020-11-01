using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Threading;
using Otor.MsixHero.Appx.Diagnostic.Logging;
using Otor.MsixHero.Appx.Diagnostic.Recommendations;
using Otor.MsixHero.Appx.Diagnostic.Recommendations.ThirdParty;
using Otor.MsixHero.Appx.Diagnostic.Registry;
using Otor.MsixHero.Appx.Diagnostic.RunningDetector;
using Otor.MsixHero.Appx.Packaging.Installation;
using Otor.MsixHero.Appx.Packaging.ModificationPackages;
using Otor.MsixHero.Appx.Packaging.Packer;
using Otor.MsixHero.Appx.Signing;
using Otor.MsixHero.Appx.Updates;
using Otor.MsixHero.Appx.Volumes;
using Otor.MsixHero.Appx.WindowsVirtualDesktop.AppAttach;
using Otor.MsixHero.Dependencies;
using Otor.MsixHero.Infrastructure.Logging;
using Otor.MsixHero.Infrastructure.Processes;
using Otor.MsixHero.Infrastructure.Processes.Ipc;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Infrastructure.Updates;
using Otor.MsixHero.Lib.Infrastructure;
using Otor.MsixHero.Lib.Infrastructure.Progress;
using Otor.MsixHero.Lib.Proxy;
using Otor.MsixHero.Ui.Hero;
using Otor.MsixHero.Ui.Hero.Executor;
using Otor.MsixHero.Ui.Modules.Common;
using Otor.MsixHero.Ui.Modules.Dialogs;
using Otor.MsixHero.Ui.Modules.EventViewer;
using Otor.MsixHero.Ui.Modules.Main;
using Otor.MsixHero.Ui.Modules.PackageList;
using Otor.MsixHero.Ui.Modules.Settings;
using Otor.MsixHero.Ui.Modules.SystemStatus;
using Otor.MsixHero.Ui.Modules.VolumeManager;
using Otor.MsixHero.Ui.Services;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace Otor.MsixHero.Ui
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
            LogManager.Initialize(MsixHeroLogLevel.Debug);
            // LogManager.Initialize(MsixHeroLogLevel.Info);
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


        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IInteractionService, InteractionService>(); 
            containerRegistry.RegisterSingleton<ISelfElevationProxyProvider<IAppxVolumeManager>, SelfElevationManagerFactory>();
            containerRegistry.RegisterSingleton<ISelfElevationProxyProvider<IRegistryManager>, SelfElevationManagerFactory>();
            containerRegistry.RegisterSingleton<ISelfElevationProxyProvider<ISigningManager>, SelfElevationManagerFactory>();
            containerRegistry.RegisterSingleton<ISelfElevationProxyProvider<IAppxLogManager>, SelfElevationManagerFactory>();
            containerRegistry.RegisterSingleton<ISelfElevationProxyProvider<IAppxPackageManager>, SelfElevationManagerFactory>();
            containerRegistry.RegisterSingleton<ISelfElevationProxyProvider<IAppAttachManager>, SelfElevationManagerFactory>();
            containerRegistry.RegisterSingleton<IAppxPacker, AppxPacker>();
            containerRegistry.RegisterSingleton<IAppxContentBuilder, AppxContentBuilder>();
            containerRegistry.RegisterSingleton<IElevatedClient, Client>();
            containerRegistry.RegisterSingleton<IBusyManager, BusyManager>();
            containerRegistry.RegisterSingleton<IConfigurationService, LocalConfigurationService>();
            containerRegistry.RegisterSingleton<IUpdateChecker, HttpUpdateChecker>();
            containerRegistry.RegisterSingleton<IAppxVolumeManager, AppxVolumeManager>();
            containerRegistry.RegisterSingleton<IAppxPackageManager, AppxPackageManager>();
            containerRegistry.RegisterSingleton<IAppxUpdateImpactAnalyzer, AppxUpdateImpactAnalyzer>();
            containerRegistry.RegisterSingleton<IMsixHeroCommandExecutor, MsixHeroCommandExecutor>();
            containerRegistry.RegisterSingleton<IMsixHeroApplication, MsixHeroApplication>();
            containerRegistry.RegisterSingleton<IRunningDetector, RunningDetector>();
            containerRegistry.RegisterSingleton<IInterProcessCommunicationManager, InterProcessCommunicationManager>();
            containerRegistry.Register<IDependencyMapper, DependencyMapper>();
            containerRegistry.Register<IThirdPartyAppProvider, ThirdPartyAppProvider>();
            containerRegistry.Register<IServiceRecommendationAdvisor, ServiceRecommendationAdvisor>();
        }
        
        protected override void OnStartup(StartupEventArgs startupEventArgs)
        {
            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentUICulture.IetfLanguageTag)));
            this.arguments = startupEventArgs.Args;
            base.OnStartup(startupEventArgs);
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            if (this.arguments.Any())
            {
                var regionManager = this.Container.Resolve<IRegionManager>();
                var handler = new ExplorerHandler(regionManager);
                handler.Handle(this.arguments.First());
            }
        }

        protected override Window CreateShell()
        {
            if (this.arguments.Any())
            {
                var shell = this.Container.Resolve<PackageExpert>();
                return shell;
            }
            else
            {

                var shell = this.Container.Resolve<MainWindow>();
                return shell;
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            var processManager = this.Container.Resolve<IInterProcessCommunicationManager>();
            processManager.Dispose();
            base.OnExit(e);
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            moduleCatalog.AddModule<MainModule>();
            moduleCatalog.AddModule<SettingsModule>();
            moduleCatalog.AddModule<PackageListModule>(); 
            moduleCatalog.AddModule<VolumeManagerModule>();
            moduleCatalog.AddModule<EventViewerModule>();
            moduleCatalog.AddModule<SystemStatusModule>();
            moduleCatalog.AddModule<DialogsModule>();
            moduleCatalog.AddModule<CommonModule>();
        }

        public void Dispose()
        {
            var processManager = IContainerProviderExtensions.Resolve<IInterProcessCommunicationManager>(this.Container);
            processManager.Dispose();
        }
    }
}
