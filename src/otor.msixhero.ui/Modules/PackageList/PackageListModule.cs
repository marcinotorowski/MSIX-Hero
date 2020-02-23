using otor.msixhero.ui.Modules.PackageList.View;
using otor.msixhero.ui.Modules.PackageList.ViewModel;
using otor.msixhero.ui.Modules.VolumeManager.View;
using otor.msixhero.ui.Modules.VolumeManager.ViewModel;
using Prism.Ioc;
using Prism.Modularity;

namespace otor.msixhero.ui.Modules.PackageList
{
    public class PackageListModule : IModule
    {
        public static string PackagesPath = "PackageList";
        public static string SidebarSingleSelection = "SidebarSingle";
        public static string SidebarEmptySelection = "SidebarEmpty";
        public static string SidebarMultiSelection = "SidebarMulti";
        public static string VolumeManagerPath = "VolumeManager";

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<PackageListView>(PackagesPath);
            containerRegistry.RegisterForNavigation<VolumeManagerView>(VolumeManagerPath);
            containerRegistry.RegisterForNavigation<SinglePackageView>(SidebarSingleSelection);
            containerRegistry.RegisterForNavigation<EmptySelectionView>(SidebarEmptySelection);
            containerRegistry.RegisterForNavigation<MultiSelectionView>(SidebarMultiSelection);

            containerRegistry.RegisterSingleton(typeof(PackageListViewModel));
            containerRegistry.RegisterSingleton(typeof(VolumeManagerViewModel));
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}
