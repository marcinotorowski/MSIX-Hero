using Otor.MsixHero.Ui.Modules.Common.PackageContent.View;
using Otor.MsixHero.Ui.Modules.Dialogs.PackageExpert.View;
using Otor.MsixHero.Ui.Modules.Dialogs.PackageExpert.ViewModel;
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
            ViewModelLocationProvider.Register<PackageExpertView, PackageExpertViewModel>();
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton(typeof(MainViewModel));
            containerRegistry.RegisterSingleton(typeof(PackageExpertViewModel));
            containerRegistry.RegisterForNavigation<MainView>(Constants.PathMain);
            containerRegistry.RegisterForNavigation<PackageExpertView>(Constants.PathPackageExpert);
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            regionManager.RegisterViewWithRegion(Constants.RegionShell, typeof(MainView));
            regionManager.RegisterViewWithRegion(Constants.RegionPackageExpertShell, typeof(PackageExpertView));

            this.regionManager.RequestNavigate(Constants.RegionShell, Constants.PathMain);
            this.regionManager.RequestNavigate(Constants.RegionPackageExpertShell, Constants.PathPackageExpert);
        }
    }
}
