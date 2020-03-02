using otor.msixhero.ui.Modules.Common.PackageContent.View;
using otor.msixhero.ui.Modules.Common.PackageContent.ViewModel;
using otor.msixhero.ui.Modules.PackageList.ViewModel;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;

namespace otor.msixhero.ui.Modules.Common
{
    public class CommonModule : IModule
    {
        public CommonModule()
        {
            ViewModelLocationProvider.Register<PackageContentView, PackageContentViewModel>();
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<PackageContentView>(Constants.PathPackagesSingleSelection);
            containerRegistry.RegisterSingleton(typeof(PackageListViewModel));
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}
