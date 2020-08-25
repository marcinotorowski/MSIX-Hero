using Otor.MsixHero.Ui.Modules.Common.PackageContent.View;
using Otor.MsixHero.Ui.Modules.Common.PackageContent.ViewModel;
using Otor.MsixHero.Ui.Modules.PackageList.ViewModel;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;

namespace Otor.MsixHero.Ui.Modules.Common
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
