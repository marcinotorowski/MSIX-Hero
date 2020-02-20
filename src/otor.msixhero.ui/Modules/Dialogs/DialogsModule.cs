using otor.msixhero.ui.Modules.Dialogs.AppAttach.View;
using otor.msixhero.ui.Modules.Dialogs.AppAttach.ViewModel;
using otor.msixhero.ui.Modules.Dialogs.AppInstaller.View;
using otor.msixhero.ui.Modules.Dialogs.AppInstaller.ViewModel;
using otor.msixhero.ui.Modules.Dialogs.CertificateExport.View;
using otor.msixhero.ui.Modules.Dialogs.CertificateExport.ViewModel;
using otor.msixhero.ui.Modules.Dialogs.EventViewer.View;
using otor.msixhero.ui.Modules.Dialogs.EventViewer.ViewModel;
using otor.msixhero.ui.Modules.Dialogs.Help;
using otor.msixhero.ui.Modules.Dialogs.Help.View;
using otor.msixhero.ui.Modules.Dialogs.Help.ViewModel;
using otor.msixhero.ui.Modules.Dialogs.ModificationPackage.View;
using otor.msixhero.ui.Modules.Dialogs.NewSelfSigned.View;
using otor.msixhero.ui.Modules.Dialogs.NewSelfSigned.ViewModel;
using otor.msixhero.ui.Modules.Dialogs.Pack.View;
using otor.msixhero.ui.Modules.Dialogs.Pack.ViewModel;
using otor.msixhero.ui.Modules.Dialogs.PackageSigning.View;
using otor.msixhero.ui.Modules.Dialogs.PackageSigning.ViewModel;
using otor.msixhero.ui.Modules.Dialogs.Unpack.View;
using otor.msixhero.ui.Modules.Dialogs.Unpack.ViewModel;
using otor.msixhero.ui.Modules.Dialogs.ModificationPackage.ViewModel;
using otor.msixhero.ui.Modules.Dialogs.Volumes.View;
using otor.msixhero.ui.Modules.Dialogs.Volumes.ViewModel;
using Prism.Ioc;
using Prism.Modularity;

namespace otor.msixhero.ui.Modules.Dialogs
{
    public class DialogsModule : IModule
    {
        public static string NewSelfSignedPath = "NewSelfSigned";
        public static string EventViewerPath = "EventViewer";
        public static string PackageSigningPath = "PackageSigning";
        public static string CertificateExportPath = "CertificateExport";
        public static string UnpackPath = "Unpack";
        public static string AppInstallerPath = "AppInstaller";
        public static string PackPath = "Pack";
        public static string AppAttachPath = "AppAttach";
        public static string ModificationPackagePath = "ModificationPackage";
        public static string HelpPath = "Help";
        public static string VolumesPath = "Volumes";

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterDialog<NewSelfSignedView, NewSelfSignedViewModel>(NewSelfSignedPath);
            containerRegistry.RegisterDialog<EventViewerView, EventViewerViewModel>(EventViewerPath);
            containerRegistry.RegisterDialog<PackageSigningView, PackageSigningViewModel>(PackageSigningPath);
            containerRegistry.RegisterDialog<CertificateExportView, CertificateExportViewModel>(CertificateExportPath);
            containerRegistry.RegisterDialog<UnpackView, UnpackViewModel>(UnpackPath);
            containerRegistry.RegisterDialog<PackView, PackViewModel>(PackPath);
            containerRegistry.RegisterDialog<HelpView, HelpViewModel>(HelpPath);
            containerRegistry.RegisterDialog<VolumesView, VolumesViewModel>(VolumesPath);
            containerRegistry.RegisterDialog<AppAttachView, AppAttachViewModel>(AppAttachPath);
            containerRegistry.RegisterDialog<ModificationPackageView, ModificationPackageViewModel>(ModificationPackagePath);
            containerRegistry.RegisterDialog<AppInstallerView, AppInstallerViewModel>(AppInstallerPath);
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}
