using Otor.MsixHero.App.Modules.Main.Sidebar.Views;
using Otor.MsixHero.App.Modules.Main.Toolbar.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace Otor.MsixHero.App.Modules.Main
{
    public class MainModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
        }
        
        public void OnInitialized(IContainerProvider containerProvider)
        {
            var regionManager = containerProvider.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion(RegionNames.Sidebar, typeof(SidebarView));
            regionManager.RegisterViewWithRegion(RegionNames.Toolbar, typeof(ToolbarView));
        }
    }
}
