using System.Windows;
using Otor.MsixHero.App.Modules;
using Otor.MsixHero.App.Modules.EventViewer;
using Otor.MsixHero.App.Modules.Main;
using Otor.MsixHero.App.Modules.Main.Shell.Views;
using Otor.MsixHero.App.Modules.Packages;
using Otor.MsixHero.App.Modules.SystemView;
using Otor.MsixHero.App.Modules.Volumes;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Regions;

namespace Otor.MsixHero.App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
        }
        
        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            moduleCatalog.AddModule(new ModuleInfo(typeof(MainModule), ModuleNames.Main, InitializationMode.WhenAvailable));
            moduleCatalog.AddModule(new ModuleInfo(typeof(PackagesModule), ModuleNames.Packages, InitializationMode.OnDemand));
            moduleCatalog.AddModule(new ModuleInfo(typeof(EventViewerModule), ModuleNames.EventViewer, InitializationMode.OnDemand));
            moduleCatalog.AddModule(new ModuleInfo(typeof(SystemViewModule), ModuleNames.SystemView, InitializationMode.OnDemand));
            moduleCatalog.AddModule(new ModuleInfo(typeof(VolumesModule), ModuleNames.Volumes, InitializationMode.OnDemand));

            base.ConfigureModuleCatalog(moduleCatalog);
        }

        protected override void Initialize()
        {
            base.Initialize();
            var regionManager = this.Container.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion(RegionNames.Root, typeof(ShellView));
        }

        protected override Window CreateShell()
        {
            var w = this.Container.Resolve<MainWindow>();
            return w;
        }
    }
}
