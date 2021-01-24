// MSIX Hero
// Copyright (C) 2021 Marcin Otorowski
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// Full notice:
// https://github.com/marcinotorowski/msix-hero/blob/develop/LICENSE.md

using System;
using System.Diagnostics;
using System.Windows.Input;
using Otor.MsixHero.App.Helpers;
using Otor.MsixHero.App.Hero.Commands.Dashboard;
using Otor.MsixHero.App.Hero.Events.Base;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Commands;
using Prism.Events;
using Prism.Modularity;
using Prism.Services.Dialogs;

namespace Otor.MsixHero.App.Modules.Dashboard.ViewModels
{
    public class DashboardViewModel : NotifyPropertyChanged
    {
        private readonly IInteractionService interactionService;
        private readonly IDialogService dialogService;
        private readonly IModuleManager moduleManager;
        private readonly DialogOpener dialogOpener;
        private string searchKey;

        public DashboardViewModel(
            IEventAggregator eventAggregator,
            IInteractionService interactionService, 
            IDialogService dialogService, 
            IModuleManager moduleManager)
        {
            this.dialogOpener = new DialogOpener(moduleManager, dialogService, interactionService);
            
            this.interactionService = interactionService;
            this.dialogService = dialogService;
            this.moduleManager = moduleManager;

            this.ShowPackDialog = new DelegateCommand(this.OnShowPackDialog);
            this.ShowAppAttachDialog = new DelegateCommand(this.OnShowAppAttachDialog);
            this.ShowAppInstallerDialog = new DelegateCommand(this.OnShowAppInstallerDialog);
            this.ShowModificationPackageDialog = new DelegateCommand(this.OnShowModificationPackageDialog);
            this.ShowWingetDialog = new DelegateCommand(this.OnShowWingetDialog);
            this.ShowUnpackDialog = new DelegateCommand(this.OnShowUnpackDialog);
            this.ShowUpdateImpactDialog = new DelegateCommand(this.OnShowUpdateImpactDialog);
            this.ShowDependencyGraphDialog = new DelegateCommand(this.OnShowDependencyGraphDialog);
            this.ShowNewSelfSignedDialog = new DelegateCommand(this.OnShowNewSelfSignedDialog);
            this.ShowExtractCertificateDialog = new DelegateCommand(this.OnShowExtractCertificateDialog);
            this.ShowSignPackageDialog = new DelegateCommand(this.OnShowSignPackageDialog);
            this.OpenFileDialog = new DelegateCommand(this.OnOpenFileDialog);
            this.OpenCertificateManager = new DelegateCommand<object>(param => this.OnOpenCertificateManager(param is bool boolParam && boolParam));
            this.OpenAppsFeatures = new DelegateCommand(this.OnOpenAppsFeatures);
            this.OpenDevSettings = new DelegateCommand(this.OnOpenDevSettings);

            eventAggregator.GetEvent<UiExecutedEvent<SetToolFilterCommand>>().Subscribe(this.OnSetToolFilterCommand);
        }

        public string SearchKey
        {
            get => this.searchKey;
            private set => this.SetField(ref this.searchKey, value);
        }

        private void OnSetToolFilterCommand(UiExecutedPayload<SetToolFilterCommand> obj)
        {
            this.SearchKey = obj.Command.SearchKey;
        }

        public ICommand ShowAppInstallerDialog { get; }

        public ICommand ShowModificationPackageDialog { get; }

        public ICommand ShowWingetDialog { get; }

        public ICommand ShowNewSelfSignedDialog { get; }

        public ICommand ShowExtractCertificateDialog { get; }

        public ICommand ShowPackDialog { get; }

        public ICommand ShowAppAttachDialog { get; }

        public ICommand ShowUnpackDialog { get; }

        public ICommand ShowUpdateImpactDialog { get; }

        public ICommand ShowDependencyGraphDialog { get; }

        public ICommand ShowSignPackageDialog { get; }

        public ICommand OpenCertificateManager { get; }
        
        public ICommand OpenFileDialog { get; }

        public ICommand OpenAppsFeatures { get; }

        public ICommand OpenDevSettings { get; }

        private void OnShowDependencyGraphDialog()
        {
            this.moduleManager.LoadModule(ModuleNames.Dialogs.Dependencies);
            this.dialogService.ShowDialog(NavigationPaths.DialogPaths.DependenciesGraph, this.OnDialogClosed);
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
            this.moduleManager.LoadModule(ModuleNames.Dialogs.Signing);
            this.dialogService.ShowDialog(NavigationPaths.DialogPaths.SigningNewSelfSigned, new DialogParameters(), this.OnDialogClosed);
        }

        private void OnShowExtractCertificateDialog()
        {
            this.moduleManager.LoadModule(ModuleNames.Dialogs.Signing);
            this.dialogService.ShowDialog(NavigationPaths.DialogPaths.SigningCertificateExport, new DialogParameters(), this.OnDialogClosed);
        }

        private void OnShowUnpackDialog()
        {
            this.moduleManager.LoadModule(ModuleNames.Dialogs.Packaging);
            this.dialogService.ShowDialog(NavigationPaths.DialogPaths.PackagingUnpack, new DialogParameters(), this.OnDialogClosed);
        }

        private void OnShowUpdateImpactDialog()
        {
            this.moduleManager.LoadModule(ModuleNames.Dialogs.Updates);
            this.dialogService.ShowDialog(NavigationPaths.DialogPaths.UpdatesUpdateImpact, new DialogParameters(), this.OnDialogClosed);
        }

        private void OnOpenFileDialog()
        {
            this.dialogOpener.ShowFileDialog(DialogOpenerType.AllSupported);
        }

        private void OnShowPackDialog()
        {
            this.moduleManager.LoadModule(ModuleNames.Dialogs.Packaging);
            this.dialogService.ShowDialog(NavigationPaths.DialogPaths.PackagingPack, new DialogParameters(), this.OnDialogClosed);
        }

        private void OnShowWingetDialog()
        {
            this.moduleManager.LoadModule(ModuleNames.Dialogs.Winget);
            this.dialogService.ShowDialog(NavigationPaths.DialogPaths.WingetYamlEditor, this.OnDialogClosed);
        }

        private void OnShowAppInstallerDialog()
        {
            this.moduleManager.LoadModule(ModuleNames.Dialogs.AppInstaller);
            this.dialogService.ShowDialog(NavigationPaths.DialogPaths.AppInstallerEditor, this.OnDialogClosed);
        }

        private void OnShowModificationPackageDialog()
        {
            this.moduleManager.LoadModule(ModuleNames.Dialogs.Packaging);
            this.dialogService.ShowDialog(NavigationPaths.DialogPaths.PackagingModificationPackage, new DialogParameters(), this.OnDialogClosed);
        }

        private void OnShowAppAttachDialog()
        {
            this.moduleManager.LoadModule(ModuleNames.Dialogs.AppAttach);
            this.dialogService.ShowDialog(NavigationPaths.DialogPaths.AppAttachEditor, new DialogParameters(), this.OnDialogClosed);
        }

        private void OnShowSignPackageDialog()
        {
            this.moduleManager.LoadModule(ModuleNames.Dialogs.Signing);
            this.dialogService.ShowDialog(NavigationPaths.DialogPaths.SigningPackageSigning, new DialogParameters(), this.OnDialogClosed);
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
