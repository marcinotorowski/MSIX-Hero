using otor.msixhero.ui.Modules.PackageList.View;
using otor.msixhero.ui.Modules.PackageList.ViewModel;
using Prism.Ioc;
using Prism.Modularity;

namespace otor.msixhero.ui.Modules.PackageList
{
    public class PackageListModule : IModule
    {
        public static string Path = "PackageList";

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<PackageListView>(Path);
            containerRegistry.RegisterSingleton(typeof(PackageListViewModel));
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}
