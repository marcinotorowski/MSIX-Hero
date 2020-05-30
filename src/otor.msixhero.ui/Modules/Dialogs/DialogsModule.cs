using otor.msixhero.ui.Modules.Dialogs.AppAttach.View;
using otor.msixhero.ui.Modules.Dialogs.AppAttach.ViewModel;
using otor.msixhero.ui.Modules.Dialogs.AppInstaller.View;
using otor.msixhero.ui.Modules.Dialogs.AppInstaller.ViewModel;
using otor.msixhero.ui.Modules.Dialogs.CertificateExport.View;
using otor.msixhero.ui.Modules.Dialogs.CertificateExport.ViewModel;
using otor.msixhero.ui.Modules.Dialogs.ChangeVolume.View;
using otor.msixhero.ui.Modules.Dialogs.ChangeVolume.ViewModel;
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
using otor.msixhero.ui.Modules.Dialogs.NewVolume.View;
using otor.msixhero.ui.Modules.Dialogs.NewVolume.ViewModel;
using otor.msixhero.ui.Modules.Dialogs.PackageExpert.View;
using otor.msixhero.ui.Modules.Dialogs.PackageExpert.ViewModel;
using otor.msixhero.ui.Modules.Dialogs.PsfExpert.Elements.View;
using otor.msixhero.ui.Modules.Dialogs.PsfExpert.Elements.ViewModel;
using otor.msixhero.ui.Modules.Dialogs.PsfExpert.View;
using otor.msixhero.ui.Modules.Dialogs.PsfExpert.ViewModel;
using otor.msixhero.ui.Modules.Dialogs.Winget.View;
using otor.msixhero.ui.Modules.Dialogs.Winget.ViewModel;
using Prism.Ioc;
using Prism.Modularity;

namespace otor.msixhero.ui.Modules.Dialogs
{
    public class DialogsModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterDialog<NewSelfSignedView, NewSelfSignedViewModel>(Constants.PathNewSelfSigned);
            containerRegistry.RegisterDialog<PsfExpertView, PsfExpertViewModel>(Constants.PathPsfExpert);
            containerRegistry.RegisterDialog<PackageSigningView, PackageSigningViewModel>(Constants.PathPackageSigning);
            containerRegistry.RegisterDialog<CertificateExportView, CertificateExportViewModel>(Constants.PathCertificateExport);
            containerRegistry.RegisterDialog<UnpackView, UnpackViewModel>(Constants.PathUnpack);
            containerRegistry.RegisterDialog<PackView, PackViewModel>(Constants.PathPack);
            containerRegistry.RegisterDialog<NewVolumeView, NewVolumeViewModel>(Constants.PathNewVolume);
            containerRegistry.RegisterDialog<ChangeVolumeView, ChangeVolumeViewModel>(Constants.PathChangeVolume);
            containerRegistry.RegisterDialog<HelpView, HelpViewModel>(Constants.PathHelp);
            containerRegistry.RegisterDialog<AppAttachView, AppAttachViewModel>(Constants.PathAppAttach);
            containerRegistry.RegisterDialog<ModificationPackageView, ModificationPackageViewModel>(Constants.PathModificationPackage);
            containerRegistry.RegisterDialog<AppInstallerView, AppInstallerViewModel>(Constants.PathAppInstaller);
            containerRegistry.RegisterDialog<PackageExpertView, PackageExpertViewModel>(Constants.PathPackageExpert);
            containerRegistry.RegisterDialog<WingetView, WingetViewModel>(Constants.PathWinget);

            containerRegistry.RegisterDialog<FileRuleView, FileRuleViewModel>(Constants.PathPackageExpertFileRule);
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}
