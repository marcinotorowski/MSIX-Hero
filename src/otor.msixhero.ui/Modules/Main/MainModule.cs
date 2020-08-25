using Otor.MsixHero.Ui.Modules.Main.View;
using Otor.MsixHero.Ui.Modules.Main.ViewModel;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Regions;

namespace Otor.MsixHero.Ui.Modules.Main
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
