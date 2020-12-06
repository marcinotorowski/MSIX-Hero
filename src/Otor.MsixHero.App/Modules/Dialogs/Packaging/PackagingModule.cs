using Otor.MsixHero.App.Controls;
using Otor.MsixHero.App.Modules.Dialogs.Packaging.ModificationPackage.View;
using Otor.MsixHero.App.Modules.Dialogs.Packaging.ModificationPackage.ViewModel;
using Otor.MsixHero.App.Modules.Dialogs.Packaging.Pack.View;
using Otor.MsixHero.App.Modules.Dialogs.Packaging.Pack.ViewModel;
using Otor.MsixHero.App.Modules.Dialogs.Packaging.Unpack.View;
using Otor.MsixHero.App.Modules.Dialogs.Packaging.Unpack.ViewModel;
using Prism.Ioc;
using Prism.Modularity;

namespace Otor.MsixHero.App.Modules.Dialogs.Packaging
{
    public class PackagingModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterDialog<ModificationPackageView, ModificationPackageViewModel>(NavigationPaths.DialogPaths.PackagingModificationPackage);
            containerRegistry.RegisterDialog<PackView, PackViewModel>(NavigationPaths.DialogPaths.PackagingPack);
            containerRegistry.RegisterDialog<UnpackView, UnpackViewModel>(NavigationPaths.DialogPaths.PackagingUnpack);
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}
