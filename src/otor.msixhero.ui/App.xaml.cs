using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;
using CommonServiceLocator;
using MSI_Hero.Domain;
using MSI_Hero.Domain.State;
using MSI_Hero.Modules.Installed;
using MSI_Hero.Modules.Installed.View;
using MSI_Hero.Modules.Installed.ViewModel;
using MSI_Hero.Modules.Main;
using MSI_Hero.Modules.Settings;
using MSI_Hero.Modules.Settings.View;
using MSI_Hero.Modules.Settings.ViewModel;
using MSI_Hero.Services;
using MSI_Hero.ViewModel;
using otor.msihero.lib;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Unity;
using Prism.Unity.Ioc;
using ApplicationState = MSI_Hero.Domain.State.ApplicationState;

namespace MSI_Hero
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton(typeof(IAppxPackageManager), typeof(AppxPackageManager));
            containerRegistry.RegisterSingleton(typeof(IBusyManager), typeof(BusyManager));
            containerRegistry.RegisterSingleton(typeof(IApplicationStateManager), typeof(ApplicationStateManager));
        }
        
        protected override Window CreateShell()
        {
            return ServiceLocator.Current.GetInstance<MainWindow>();
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            moduleCatalog.AddModule<MainModule>();
            moduleCatalog.AddModule<SettingsModule>();
            moduleCatalog.AddModule<PackageListModule>();
        }
        
        protected override void ConfigureViewModelLocator()
        {
            base.ConfigureViewModelLocator();
            ViewModelLocationProvider.Register<MainView, MainViewModel>();
            ViewModelLocationProvider.Register<PackageListView, PackageListViewModel>();
        }
    }
}
