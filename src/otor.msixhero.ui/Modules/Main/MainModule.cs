using otor.msixhero.ui.Modules.Main.View;
using otor.msixhero.ui.Modules.Main.ViewModel;
using otor.msixhero.ui.Modules.PackageList;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Regions;

namespace otor.msixhero.ui.Modules.Main
{
    public class MainModule : IModule
    {
        private readonly IRegionManager regionManager;

        public MainModule(IRegionManager regionManager)
        {
            this.regionManager = regionManager;
            ViewModelLocationProvider.Register<MainView, MainViewModel>();
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton(typeof(MainViewModel));
            containerRegistry.RegisterForNavigation<MainView>(Constants.PathMain);
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            regionManager.RegisterViewWithRegion(Constants.RegionShell, typeof(MainView));
            this.regionManager.RequestNavigate(Constants.RegionShell, Constants.PathMain);
        }
    }
}
