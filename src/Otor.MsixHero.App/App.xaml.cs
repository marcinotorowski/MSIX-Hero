using System;
using System.Windows;
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Hero.Commands;
using Otor.MsixHero.App.Hero.Executor;
using Otor.MsixHero.App.Hero.State;
using Otor.MsixHero.App.Modules;
using Otor.MsixHero.App.Modules.Dashboard;
using Otor.MsixHero.App.Modules.Dialogs.AppAttach;
using Otor.MsixHero.App.Modules.Dialogs.AppInstaller;
using Otor.MsixHero.App.Modules.Dialogs.Dependencies;
using Otor.MsixHero.App.Modules.Dialogs.Packaging;
using Otor.MsixHero.App.Modules.Dialogs.Settings;
using Otor.MsixHero.App.Modules.Dialogs.Signing;
using Otor.MsixHero.App.Modules.Dialogs.Updates;
using Otor.MsixHero.App.Modules.Dialogs.Winget;
using Otor.MsixHero.App.Modules.EventViewer;
using Otor.MsixHero.App.Modules.Main;
using Otor.MsixHero.App.Modules.Main.Shell.Views;
using Otor.MsixHero.App.Modules.PackageManagement;
using Otor.MsixHero.App.Modules.SystemStatus;
using Otor.MsixHero.App.Modules.VolumeManagement;
using Otor.MsixHero.App.Services;
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
using Otor.MsixHero.Infrastructure.Processes;
using Otor.MsixHero.Infrastructure.Processes.Ipc;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Infrastructure.Updates;
using Otor.MsixHero.Lib.Infrastructure.Progress;
using Otor.MsixHero.Lib.Proxy;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace Otor.MsixHero.App
{
    /// <summary>
    /// Interaction logic for the application.
    /// </summary>
    public partial class App : IDisposable
    {
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
            containerRegistry.RegisterSingleton<PrismServices>();
        }
        
        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            moduleCatalog.AddModule(new ModuleInfo(typeof(MainModule), ModuleNames.Main, InitializationMode.WhenAvailable));
            moduleCatalog.AddModule(new ModuleInfo(typeof(PackageManagementModule), ModuleNames.PackageManagement, InitializationMode.OnDemand));
            moduleCatalog.AddModule(new ModuleInfo(typeof(EventViewerModule), ModuleNames.EventViewer, InitializationMode.OnDemand));
            moduleCatalog.AddModule(new ModuleInfo(typeof(SystemStatusModule), ModuleNames.SystemStatus, InitializationMode.OnDemand));
            moduleCatalog.AddModule(new ModuleInfo(typeof(VolumeManagementModule), ModuleNames.VolumeManagement, InitializationMode.OnDemand));
            moduleCatalog.AddModule(new ModuleInfo(typeof(DashboardModule), ModuleNames.Dashboard, InitializationMode.OnDemand));
            
            moduleCatalog.AddModule(new ModuleInfo(typeof(SigningModule), ModuleNames.Dialogs.Signing, InitializationMode.OnDemand));
            moduleCatalog.AddModule(new ModuleInfo(typeof(AppInstallerModule), ModuleNames.Dialogs.AppInstaller, InitializationMode.OnDemand));
            moduleCatalog.AddModule(new ModuleInfo(typeof(PackagingModule), ModuleNames.Dialogs.Packaging, InitializationMode.OnDemand));
            moduleCatalog.AddModule(new ModuleInfo(typeof(DependenciesModule), ModuleNames.Dialogs.Dependencies, InitializationMode.OnDemand));
            moduleCatalog.AddModule(new ModuleInfo(typeof(UpdatesModule), ModuleNames.Dialogs.Updates, InitializationMode.OnDemand));
            moduleCatalog.AddModule(new ModuleInfo(typeof(Modules.Dialogs.Volumes.VolumesModule), ModuleNames.Dialogs.Volumes, InitializationMode.OnDemand));
            moduleCatalog.AddModule(new ModuleInfo(typeof(WingetModule), ModuleNames.Dialogs.Winget, InitializationMode.OnDemand));
            moduleCatalog.AddModule(new ModuleInfo(typeof(AppAttachModule), ModuleNames.Dialogs.AppAttach, InitializationMode.OnDemand));
            moduleCatalog.AddModule(new ModuleInfo(typeof(SettingsModule), ModuleNames.Dialogs.Settings, InitializationMode.OnDemand));
            
            base.ConfigureModuleCatalog(moduleCatalog);
        }

        protected override void Initialize()
        {
            base.Initialize();
            var regionManager = this.Container.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion(RegionNames.Root, typeof(ShellView));

            var app = this.Container.Resolve<IMsixHeroApplication>();
            app.CommandExecutor.Invoke(null, new SetCurrentModeCommand(ApplicationMode.Packages));
        }

        protected override Window CreateShell()
        {
            var w = this.Container.Resolve<MainWindow>();
            return w;
        }

        public void Dispose()
        {
            var processManager = this.Container.Resolve<IInterProcessCommunicationManager>();
            processManager.Dispose();
        }
    }
}
