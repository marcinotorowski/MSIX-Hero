using System;
using System.Windows;
using CommonServiceLocator;
using otor.msixhero.lib;
using otor.msixhero.lib.BusinessLayer.Appx;
using otor.msixhero.lib.BusinessLayer.Appx.Signing;
using otor.msixhero.lib.BusinessLayer.Commanding;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Configuration;
using otor.msixhero.lib.Infrastructure.Interop;
using otor.msixhero.lib.Infrastructure.Ipc;
using otor.msixhero.lib.Infrastructure.Logging;
using otor.msixhero.ui.Modules.Dialogs;
using otor.msixhero.ui.Modules.Dialogs.NewSelfSigned.View;
using otor.msixhero.ui.Modules.Dialogs.NewSelfSigned.ViewModel;
using otor.msixhero.ui.Modules.Main;
using otor.msixhero.ui.Modules.Main.View;
using otor.msixhero.ui.Modules.Main.ViewModel;
using otor.msixhero.ui.Modules.PackageList;
using otor.msixhero.ui.Modules.PackageList.View;
using otor.msixhero.ui.Modules.PackageList.ViewModel;
using otor.msixhero.ui.Modules.Settings;
using otor.msixhero.ui.Services;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Unity;

namespace otor.msixhero.ui
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger();

        static App()
        {
#if DEBUG
            LogManager.Initialize(MsixHeroLogLevel.Debug);
#else
            LogManager.Initialize(MsixHeroLogLevel.Info);
#endif
        }

        private readonly IProcessManager processManager = new ProcessManager();

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IInteractionService, InteractionService>();
            containerRegistry.RegisterSingleton<IAppxSigningManager, AppxSigningManager>();
            containerRegistry.RegisterSingleton<IAppxPacker, AppxPacker>();
            containerRegistry.RegisterSingleton<IAppxPackageManagerFactory, AppxPackageManagerFactory>();
            containerRegistry.RegisterSingleton<IBusyManager, BusyManager>();
            containerRegistry.RegisterSingleton<IClientCommandRemoting, ClientCommandRemoting>();
            containerRegistry.RegisterSingleton<IConfigurationService, LocalConfigurationService>();
            containerRegistry.RegisterSingleton<IApplicationStateManager, ApplicationStateManager>();
            containerRegistry.RegisterInstance<IProcessManager>(this.processManager);
        }
        
        protected override Window CreateShell()
        {
            var shell = ServiceLocator.Current.GetInstance<MainWindow>();
            return shell;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (this.Container.Resolve<IApplicationStateManager>() is IDisposable appStateManager)
            {
                appStateManager.Dispose();
            }

            this.processManager.Dispose();
            base.OnExit(e);
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            moduleCatalog.AddModule<MainModule>();
            moduleCatalog.AddModule<SettingsModule>();
            moduleCatalog.AddModule<PackageListModule>();
            moduleCatalog.AddModule<DialogsModule>();
        }
        
        protected override void ConfigureViewModelLocator()
        {
            base.ConfigureViewModelLocator();
            ViewModelLocationProvider.Register<MainView, MainViewModel>();
            ViewModelLocationProvider.Register<PackageListView, PackageListViewModel>();
            ViewModelLocationProvider.Register<SinglePackageView, SinglePackageViewModel>();
            ViewModelLocationProvider.Register<MultiSelectionView, MultiSelectionViewModel>();
            ViewModelLocationProvider.Register<NewSelfSignedView, NewSelfSignedViewModel>();
        }

        public void Dispose()
        {
            this.processManager.Dispose();
        }
    }
}
