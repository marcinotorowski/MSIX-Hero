using System;
using System.Diagnostics;
using System.Windows.Input;
using Otor.MsixHero.App.Modules.Editors;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Commands;
using Prism.Modularity;
using Prism.Services.Dialogs;

namespace Otor.MsixHero.App.Modules.Overview.ViewModels
{
    public class OverviewViewModel
    {
        private readonly IInteractionService interactionService;
        private readonly IDialogService dialogService;
        private readonly IModuleManager moduleManager;

        public OverviewViewModel(IInteractionService interactionService, IDialogService dialogService, IModuleManager moduleManager)
        {
            this.interactionService = interactionService;
            this.dialogService = dialogService;
            this.moduleManager = moduleManager;

            this.ShowPackDialog = new DelegateCommand(this.OnShowPackDialog);
            this.ShowAppAttachDialog = new DelegateCommand(this.OnShowAppAttachDialog);
            this.ShowAppInstallerDialog = new DelegateCommand(this.OnShowAppInstallerDialog);
            this.ShowModificationPackageDialog = new DelegateCommand(this.OnShowModificationPackageDialog);
            this.ShowUnpackDialog = new DelegateCommand(this.OnShowUnpackDialog);
            this.ShowUpdateImpactDialog = new DelegateCommand(this.OnShowUpdateImpactDialog);
            this.ShowDependencyGraphDialog = new DelegateCommand(this.OnShowDependencyGraphDialog);
            this.ShowWingetDialog = new DelegateCommand(this.OnShowWingetDialog);
            this.ShowNewSelfSignedDialog = new DelegateCommand(this.OnShowNewSelfSignedDialog);
            this.ShowExtractCertificateDialog = new DelegateCommand(this.OnShowExtractCertificateDialog);
            this.ShowSignPackageDialog = new DelegateCommand(this.OnShowSignPackageDialog);
            this.OpenCertificateManager = new DelegateCommand<object>(param => this.OnOpenCertificateManager(param is bool boolParam && boolParam));
            this.OpenAppsFeatures = new DelegateCommand(this.OnOpenAppsFeatures);
            this.OpenDevSettings = new DelegateCommand(this.OnOpenDevSettings);
        }

        public ICommand ShowNewSelfSignedDialog { get; }

        public ICommand ShowExtractCertificateDialog { get; }

        public ICommand ShowPackDialog { get; }

        public ICommand ShowAppInstallerDialog { get; }

        public ICommand ShowAppAttachDialog { get; }

        public ICommand ShowModificationPackageDialog { get; }

        public ICommand ShowUnpackDialog { get; }

        public ICommand ShowUpdateImpactDialog { get; }

        public ICommand ShowDependencyGraphDialog { get; }

        public ICommand ShowWingetDialog { get; }

        public ICommand ShowSignPackageDialog { get; }

        public ICommand OpenCertificateManager { get; }

        public ICommand OpenAppsFeatures { get; }

        public ICommand OpenDevSettings { get; }

        private void OnShowDependencyGraphDialog()
        {
            this.moduleManager.LoadModule(DialogModuleNames.Dependencies);
            this.DialogExecute("All supported files|*.msix;*.appx;AppxManifest.xml|Packages|*.msix;*.appx|Manifest files|AppxManifest.xml", DialogPathNames.DependenciesGraph);
        }

        private void OnOpenAppsFeatures()
        {
            var process = new ProcessStartInfo("ms-settings:appsfeatures") { UseShellExecute = true };
            Process.Start(process);
        }

        private void OnOpenDevSettings()
        {
            var process = new ProcessStartInfo("ms-settings:developers") { UseShellExecute = true };
            Process.Start(process);
        }

        private void OnShowNewSelfSignedDialog()
        {
            this.moduleManager.LoadModule(DialogModuleNames.Signing);
            this.dialogService.ShowDialog(DialogPathNames.SigningNewSelfSigned, new DialogParameters(), this.OnDialogClosed);
        }

        private void OnShowExtractCertificateDialog()
        {
            this.moduleManager.LoadModule(DialogModuleNames.Signing);
            this.dialogService.ShowDialog(DialogPathNames.SigningCertificateExport, new DialogParameters(), this.OnDialogClosed);
        }

        private void OnShowUnpackDialog()
        {
            this.moduleManager.LoadModule(DialogModuleNames.Packaging);
            this.dialogService.ShowDialog(DialogPathNames.PackagingUnpack, new DialogParameters(), this.OnDialogClosed);
        }

        private void OnShowUpdateImpactDialog()
        {
            this.moduleManager.LoadModule(DialogModuleNames.Updates);
            this.dialogService.ShowDialog(DialogPathNames.UpdatesUpdateImpact, new DialogParameters(), this.OnDialogClosed);
        }

        private void DialogExecute(string filter, string path, string parameterName = "file")
        {
            if (!this.interactionService.SelectFile(filter, out var selected))
            {
                return;
            }

            var parameters = new DialogParameters
            {
                { parameterName, selected }
            };

            this.dialogService.ShowDialog(path, parameters, this.OnDialogClosed);
        }

        private void OnShowPackDialog()
        {
            this.moduleManager.LoadModule(DialogModuleNames.Packaging);
            this.dialogService.ShowDialog(DialogPathNames.PackagingPack, new DialogParameters(), this.OnDialogClosed);
        }

        private void OnShowWingetDialog()
        {
            this.moduleManager.LoadModule(DialogModuleNames.Winget);
            this.DialogExecute("Winget manifests (*.yaml)|*.yaml|All files|*.*", DialogPathNames.WingetYamlEditor, "yaml");
        }

        private void OnShowAppInstallerDialog()
        {
            this.moduleManager.LoadModule(DialogModuleNames.AppInstaller);
            this.DialogExecute("App installer files|*.appinstaller|All files|*.*", DialogPathNames.AppInstallerEditor);
        }

        private void OnShowModificationPackageDialog()
        {
            this.moduleManager.LoadModule(DialogModuleNames.Packaging);
            this.dialogService.ShowDialog(DialogPathNames.PackagingModificationPackage, new DialogParameters(), this.OnDialogClosed);
        }

        private void OnShowAppAttachDialog()
        {
            this.moduleManager.LoadModule(DialogModuleNames.AppAttach);
            this.dialogService.ShowDialog(DialogPathNames.AppAttachEditor, new DialogParameters(), this.OnDialogClosed);
        }

        private void OnShowSignPackageDialog()
        {
            this.moduleManager.LoadModule(DialogModuleNames.Signing);
            this.dialogService.ShowDialog(DialogPathNames.SigningPackageSigning, new DialogParameters(), this.OnDialogClosed);
        }

        private void OnOpenCertificateManager(bool perMachine)
        {
            try
            {
                ProcessStartInfo psi;
                if (perMachine)
                {
                    psi = new ProcessStartInfo("mmc.exe", "certlm.msc");
                }
                else
                {
                    psi = new ProcessStartInfo("mmc.exe", "certmgr.msc");

                }

                psi.Verb = "runas";
                psi.UseShellExecute = true;
                Process.Start(psi);
            }
            catch (Exception e)
            {
                this.interactionService.ShowError(e.Message, e);
            }
        }

        private void OnDialogClosed(IDialogResult obj)
        {
        }
    }
}