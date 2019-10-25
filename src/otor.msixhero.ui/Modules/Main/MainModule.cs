using otor.msixhero.ui.Modules.Main.View;
using otor.msixhero.ui.Modules.Main.ViewModel;
using Prism.Ioc;
using Prism.Modularity;

namespace otor.msixhero.ui.Modules.Main
{
    public class MainModule : IModule
    {
        public static string Path = "Main";

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<MainView>(Path);
            containerRegistry.RegisterSingleton(typeof(MainViewModel));
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}
