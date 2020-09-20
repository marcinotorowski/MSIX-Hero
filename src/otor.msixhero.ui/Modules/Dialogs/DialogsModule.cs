using Otor.MsixHero.Ui.Modules.Dialogs.AppAttach.View;
using Otor.MsixHero.Ui.Modules.Dialogs.AppAttach.ViewModel;
using Otor.MsixHero.Ui.Modules.Dialogs.AppInstaller.View;
using Otor.MsixHero.Ui.Modules.Dialogs.AppInstaller.ViewModel;
using Otor.MsixHero.Ui.Modules.Dialogs.CertificateExport.View;
using Otor.MsixHero.Ui.Modules.Dialogs.CertificateExport.ViewModel;
using Otor.MsixHero.Ui.Modules.Dialogs.ChangeVolume.View;
using Otor.MsixHero.Ui.Modules.Dialogs.ChangeVolume.ViewModel;
using Otor.MsixHero.Ui.Modules.Dialogs.Help.View;
using Otor.MsixHero.Ui.Modules.Dialogs.Help.ViewModel;
using Otor.MsixHero.Ui.Modules.Dialogs.ModificationPackage.View;
using Otor.MsixHero.Ui.Modules.Dialogs.ModificationPackage.ViewModel;
using Otor.MsixHero.Ui.Modules.Dialogs.NewSelfSigned.View;
using Otor.MsixHero.Ui.Modules.Dialogs.NewSelfSigned.ViewModel;
using Otor.MsixHero.Ui.Modules.Dialogs.NewVolume.View;
using Otor.MsixHero.Ui.Modules.Dialogs.NewVolume.ViewModel;
using Otor.MsixHero.Ui.Modules.Dialogs.Pack.View;
using Otor.MsixHero.Ui.Modules.Dialogs.Pack.ViewModel;
using Otor.MsixHero.Ui.Modules.Dialogs.PackageExpert.View;
using Otor.MsixHero.Ui.Modules.Dialogs.PackageExpert.ViewModel;
using Otor.MsixHero.Ui.Modules.Dialogs.PackageSigning.View;
using Otor.MsixHero.Ui.Modules.Dialogs.PackageSigning.ViewModel;
using Otor.MsixHero.Ui.Modules.Dialogs.PsfExpert.Elements.View;
using Otor.MsixHero.Ui.Modules.Dialogs.PsfExpert.Elements.ViewModel;
using Otor.MsixHero.Ui.Modules.Dialogs.PsfExpert.View;
using Otor.MsixHero.Ui.Modules.Dialogs.PsfExpert.ViewModel;
using Otor.MsixHero.Ui.Modules.Dialogs.Unpack.View;
using Otor.MsixHero.Ui.Modules.Dialogs.Unpack.ViewModel;
using Otor.MsixHero.Ui.Modules.Dialogs.UpdateImpact.View;
using Otor.MsixHero.Ui.Modules.Dialogs.UpdateImpact.ViewModel;
using Otor.MsixHero.Ui.Modules.Dialogs.Winget.View;
using Otor.MsixHero.Ui.Modules.Dialogs.Winget.ViewModel;
using Otor.MsixHero.Ui.Modules.VolumeManager.View;
using Otor.MsixHero.Ui.Modules.VolumeManager.ViewModel;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;

namespace Otor.MsixHero.Ui.Modules.Dialogs
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
            containerRegistry.RegisterDialog<UpdateImpactView, UpdateImpactViewModel>(Constants.PathUpdateImpact);
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
