using System;
using System.Windows;
using CommonServiceLocator;
using otor.msixhero.lib;
using otor.msixhero.lib.BusinessLayer.Infrastructure;
using otor.msixhero.lib.BusinessLayer.Infrastructure.Implementation;
using otor.msixhero.lib.Ipc;
using otor.msixhero.lib.Managers;
using otor.msixhero.lib.Services;
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
        private readonly IProcessManager processManager = new ProcessManager();

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IInteractionService, InteractionService>();
            containerRegistry.RegisterSingleton<IAppxSigningManager, AppxSigningManager>();
            containerRegistry.RegisterSingleton<IAppxPackageManagerFactory, AppxPackageManagerFactory>();
            containerRegistry.RegisterSingleton<IBusyManager, BusyManager>();
            containerRegistry.RegisterSingleton<IClientCommandRemoting, ClientCommandRemoting>();
            containerRegistry.RegisterSingleton<IConfigurationService, ConfigurationService>();
            containerRegistry.RegisterSingleton<IApplicationStateManager, ApplicationStateManager>();
            containerRegistry.RegisterInstance<IProcessManager>(this.processManager);
        }
        
        protected override Window CreateShell()
        {
            var appStateManager = ServiceLocator.Current.GetInstance<IApplicationStateManager>();
            appStateManager.Initialize().GetAwaiter().GetResult();

            var shell = ServiceLocator.Current.GetInstance<MainWindow>();
            return shell;
        }

        protected override void OnExit(ExitEventArgs e)
        {
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
