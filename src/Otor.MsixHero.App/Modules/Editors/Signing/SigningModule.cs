
using Otor.MsixHero.App.Modules.Editors.Signing.CertificateExport.View;
using Otor.MsixHero.App.Modules.Editors.Signing.CertificateExport.ViewModel;
using Otor.MsixHero.App.Modules.Editors.Signing.NewSelfSigned.View;
using Otor.MsixHero.App.Modules.Editors.Signing.NewSelfSigned.ViewModel;
using Otor.MsixHero.App.Modules.Editors.Signing.PackageSigning.View;
using Otor.MsixHero.App.Modules.Editors.Signing.PackageSigning.ViewModel;
using Prism.Ioc;
using Prism.Modularity;

namespace Otor.MsixHero.App.Modules.Editors.Signing
{
    public class SigningModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<CertificateExportView, CertificateExportViewModel>(DialogPathNames.SigningCertificateExport);
            containerRegistry.RegisterForNavigation<NewSelfSignedView, NewSelfSignedViewModel>(DialogPathNames.SigningNewSelfSigned);
            containerRegistry.RegisterForNavigation<PackageSigningView, PackageSigningViewModel>(DialogPathNames.SigningPackageSigning);
           
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}
