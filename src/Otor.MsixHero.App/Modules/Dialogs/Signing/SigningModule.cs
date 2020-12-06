using Otor.MsixHero.App.Controls;
using Otor.MsixHero.App.Modules.Dialogs.Signing.CertificateExport.View;
using Otor.MsixHero.App.Modules.Dialogs.Signing.CertificateExport.ViewModel;
using Otor.MsixHero.App.Modules.Dialogs.Signing.NewSelfSigned.View;
using Otor.MsixHero.App.Modules.Dialogs.Signing.NewSelfSigned.ViewModel;
using Otor.MsixHero.App.Modules.Dialogs.Signing.PackageSigning.View;
using Otor.MsixHero.App.Modules.Dialogs.Signing.PackageSigning.ViewModel;
using Prism.Ioc;
using Prism.Modularity;

namespace Otor.MsixHero.App.Modules.Dialogs.Signing
{
    public class SigningModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterDialog<CertificateExportView, CertificateExportViewModel>(NavigationPaths.DialogPaths.SigningCertificateExport);
            containerRegistry.RegisterDialog<NewSelfSignedView, NewSelfSignedViewModel>(NavigationPaths.DialogPaths.SigningNewSelfSigned);
            containerRegistry.RegisterDialog<PackageSigningView, PackageSigningViewModel>(NavigationPaths.DialogPaths.SigningPackageSigning);
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}
