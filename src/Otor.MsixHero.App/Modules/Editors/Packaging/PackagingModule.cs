using Otor.MsixHero.App.Modules.Editors.Packaging.ModificationPackage.View;
using Otor.MsixHero.App.Modules.Editors.Packaging.ModificationPackage.ViewModel;
using Otor.MsixHero.App.Modules.Editors.Packaging.Pack.View;
using Otor.MsixHero.App.Modules.Editors.Packaging.Pack.ViewModel;
using Otor.MsixHero.App.Modules.Editors.Packaging.Unpack.View;
using Otor.MsixHero.App.Modules.Editors.Packaging.Unpack.ViewModel;
using Prism.Ioc;
using Prism.Modularity;

namespace Otor.MsixHero.App.Modules.Editors.Packaging
{
    public class PackagingModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<ModificationPackageView, ModificationPackageViewModel>(DialogPathNames.PackagingModificationPackage);
            containerRegistry.RegisterForNavigation<PackView, PackViewModel>(DialogPathNames.PackagingPack);
            containerRegistry.RegisterForNavigation<UnpackView, UnpackViewModel>(DialogPathNames.PackagingUnpack);
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}
