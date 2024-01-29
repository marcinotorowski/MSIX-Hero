// MSIX Hero
// Copyright (C) 2024 Marcin Otorowski
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
using Otor.MsixHero.App.Helpers.Dialogs;
using Otor.MsixHero.App.Hero.Commands.Tools;
using Otor.MsixHero.App.Hero.Events.Base;
using Otor.MsixHero.App.Modules.Dialogs.Packaging.SharedPackageContainer.Navigation;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Commands;
using Prism.Events;
using Prism.Modularity;
using Prism.Services.Dialogs;

namespace Otor.MsixHero.App.Modules.Tools.ViewModels
{
    public class ToolViewModel : NotifyPropertyChanged
    {
        private readonly IInteractionService _interactionService;
        private readonly IDialogService _dialogService;
        private readonly IModuleManager _moduleManager;
        private readonly DialogOpener _dialogOpener;
        private string _searchKey;

        public ToolViewModel(
            IEventAggregator eventAggregator,
            IInteractionService interactionService, 
            IDialogService dialogService,
            IModuleManager moduleManager)
        {
            this._dialogOpener = new DialogOpener(moduleManager, dialogService, interactionService);
            
            this._interactionService = interactionService;
            this._dialogService = dialogService;
            this._moduleManager = moduleManager;

            this.ShowAppAttachDialog = new DelegateCommand(this.OnShowAppAttachDialog);
            this.ShowAppInstallerDialog = new DelegateCommand<object>(this.OnShowAppInstallerDialog);
            this.ShowWingetDialog = new DelegateCommand<object>(this.OnShowWingetDialog);
            
            this.ShowPackDialog = new DelegateCommand(this.OnShowPackDialog);
            this.ShowNamesDialog = new DelegateCommand(this.OnShowNamesDialog);
            this.ShowModificationPackageDialog = new DelegateCommand(this.OnShowModificationPackageDialog);
            this.ShowUnpackDialog = new DelegateCommand(this.OnShowUnpackDialog);
            this.ShowSharedPackageContainerDialog = new DelegateCommand(this.OnShowSharedPackageContainerDialog);
            this.ShowUpdateImpactDialog = new DelegateCommand(this.OnShowUpdateImpactDialog);
            this.ShowDependencyGraphDialog = new DelegateCommand(this.OnShowDependencyGraphDialog);
            this.ShowNewSelfSignedDialog = new DelegateCommand(this.OnShowNewSelfSignedDialog);
            this.ShowExtractCertificateDialog = new DelegateCommand(this.OnShowExtractCertificateDialog);
            this.ShowSignPackageDialog = new DelegateCommand(this.OnShowSignPackageDialog);
            this.OpenFileDialog = new DelegateCommand(this.OnOpenFileDialog);
            this.OpenMsixDialog = new DelegateCommand(this.OnOpenMsixDialog);
            this.OpenCertificateManager = new DelegateCommand<object>(param => this.OnOpenCertificateManager(param is bool boolParam && boolParam));
            this.OpenAppsFeatures = new DelegateCommand(this.OnOpenAppsFeatures);
            this.OpenDevSettings = new DelegateCommand(this.OnOpenDevSettings);

            eventAggregator.GetEvent<UiExecutedEvent<SetToolFilterCommand>>().Subscribe(this.OnSetToolFilterCommand);
        }
        
        public string SearchKey
        {
            get => this._searchKey;
            private set => this.SetField(ref this._searchKey, value);
        }

        private void OnSetToolFilterCommand(UiExecutedPayload<SetToolFilterCommand> obj)
        {
            this.SearchKey = obj.Request.SearchKey;
        }

        public ICommand ShowAppInstallerDialog { get; }

        public ICommand ShowModificationPackageDialog { get; }

        public ICommand ShowWingetDialog { get; }

        public ICommand ShowNewSelfSignedDialog { get; }

        public ICommand ShowExtractCertificateDialog { get; }

        public ICommand ShowPackDialog { get; }

        public ICommand ShowAppAttachDialog { get; }

        public ICommand ShowUnpackDialog { get; }

        public ICommand ShowSharedPackageContainerDialog { get; }

        public ICommand ShowUpdateImpactDialog { get; }

        public ICommand ShowNamesDialog { get; }

        public ICommand ShowDependencyGraphDialog { get; }

        public ICommand ShowSignPackageDialog { get; }

        public ICommand OpenCertificateManager { get; }
        
        public ICommand OpenFileDialog { get; }
        
        public ICommand OpenMsixDialog { get; }

        public ICommand OpenAppsFeatures { get; }

        public ICommand OpenDevSettings { get; }

        private void OnShowDependencyGraphDialog()
        {
            this._moduleManager.LoadModule(ModuleNames.Dialogs.Dependencies);
            this._dialogService.ShowDialog(NavigationPaths.DialogPaths.DependenciesGraph, this.OnDialogClosed);
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
            this._moduleManager.LoadModule(ModuleNames.Dialogs.Signing);
            this._dialogService.ShowDialog(NavigationPaths.DialogPaths.SigningNewSelfSigned, new DialogParameters(), this.OnDialogClosed);
        }

        private void OnShowExtractCertificateDialog()
        {
            this._moduleManager.LoadModule(ModuleNames.Dialogs.Signing);
            this._dialogService.ShowDialog(NavigationPaths.DialogPaths.SigningCertificateExport, new DialogParameters(), this.OnDialogClosed);
        }

        private void OnShowUnpackDialog()
        {
            this._moduleManager.LoadModule(ModuleNames.Dialogs.Packaging);
            this._dialogService.ShowDialog(NavigationPaths.DialogPaths.PackagingUnpack, new DialogParameters(), this.OnDialogClosed);
        }

        private void OnShowSharedPackageContainerDialog()
        {
            this._moduleManager.LoadModule(ModuleNames.Dialogs.Packaging);
            
            var dialogRequest = new NavigationRequest();
            this._dialogService.ShowDialog(NavigationPaths.DialogPaths.PackagingSharedPackageContainer, dialogRequest.ToDialogParameters(), this.OnDialogClosed);
        }

        private void OnShowUpdateImpactDialog()
        {
            this._moduleManager.LoadModule(ModuleNames.Dialogs.Updates);
            this._dialogService.ShowDialog(NavigationPaths.DialogPaths.UpdatesUpdateImpact, new DialogParameters(), this.OnDialogClosed);
        }

        private void OnOpenFileDialog()
        {
            // ReSharper disable once StringLiteralTypo
            var filterBuilder = new DialogFilterBuilder().WithPackages().WithManifests().WithWinget().WithAppInstaller().WithAll().WithAllSupported();
            this._dialogOpener.ShowFileDialog(filterBuilder);
        }

        private void OnOpenMsixDialog()
        {
            // ReSharper disable once StringLiteralTypo
            var filterBuilder = new DialogFilterBuilder().WithPackages().WithAll();
            this._dialogOpener.ShowFileDialog(filterBuilder);
        }

        private void OnShowPackDialog()
        {
            this._moduleManager.LoadModule(ModuleNames.Dialogs.Packaging);
            this._dialogService.ShowDialog(NavigationPaths.DialogPaths.PackagingPack, new DialogParameters(), this.OnDialogClosed);
        }

        private void OnShowNamesDialog()
        {
            this._moduleManager.LoadModule(ModuleNames.Dialogs.Packaging);
            this._dialogService.ShowDialog(NavigationPaths.DialogPaths.PackagingNames, new DialogParameters(), this.OnDialogClosed);
        }

        private void OnShowModificationPackageDialog()
        {
            this._moduleManager.LoadModule(ModuleNames.Dialogs.Packaging);
            this._dialogService.ShowDialog(NavigationPaths.DialogPaths.PackagingModificationPackage, new DialogParameters(), this.OnDialogClosed);
        }

        private void OnShowWingetDialog(object param)
        {
            if (param != null)
            {
                var fileFilter = new DialogFilterBuilder().WithWinget().WithAll();
                var settings = new FileDialogSettings
                {
                    Filter = fileFilter,
                    DialogTitle = string.Format(Resources.Localization.Dashboard_OpenWinget_TitleFormat, $"*{FileConstants.WingetExtension}")
                };

                if (!this._interactionService.SelectFile(settings, out var filePath))
                {
                    return;
                }

                this._moduleManager.LoadModule(ModuleNames.Dialogs.Winget);
                IDialogParameters parameters = new DialogParameters();
                parameters.Add("yaml", filePath);
                this._dialogService.ShowDialog(NavigationPaths.DialogPaths.WingetYamlEditor, parameters, this.OnDialogClosed);
            }
            else
            {
                this._moduleManager.LoadModule(ModuleNames.Dialogs.Winget);
                this._dialogService.ShowDialog(NavigationPaths.DialogPaths.WingetYamlEditor, this.OnDialogClosed);
            }
        }

        private void OnShowAppInstallerDialog(object param)
        {
            if (param != null)
            {
                var fileFilter = new DialogFilterBuilder().WithAppInstaller().WithAll();
                var settings = new FileDialogSettings
                {
                    Filter = fileFilter,
                    DialogTitle = string.Format(Resources.Localization.Dashboard_OpenAppInstaller_TitleFormat, $"*{FileConstants.AppInstallerExtension}")
                };

                if (!this._interactionService.SelectFile(settings, out var filePath))
                {
                    return;
                }

                this._moduleManager.LoadModule(ModuleNames.Dialogs.AppInstaller);
                IDialogParameters parameters = new DialogParameters();
                parameters.Add("file", filePath);
                this._dialogService.ShowDialog(NavigationPaths.DialogPaths.AppInstallerEditor, parameters, this.OnDialogClosed);
            }
            else
            {
                this._moduleManager.LoadModule(ModuleNames.Dialogs.AppInstaller);
                this._dialogService.ShowDialog(NavigationPaths.DialogPaths.AppInstallerEditor, this.OnDialogClosed);
            }
        }

        private void OnShowAppAttachDialog()
        {
            this._moduleManager.LoadModule(ModuleNames.Dialogs.AppAttach);
            this._dialogService.ShowDialog(NavigationPaths.DialogPaths.AppAttachEditor, new DialogParameters(), this.OnDialogClosed);
        }

        private void OnShowSignPackageDialog()
        {
            this._moduleManager.LoadModule(ModuleNames.Dialogs.Signing);
            this._dialogService.ShowDialog(NavigationPaths.DialogPaths.SigningPackageSigning, new DialogParameters(), this.OnDialogClosed);
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
                this._interactionService.ShowError(e.Message, e);
            }
        }

        private void OnDialogClosed(IDialogResult obj)
        {
        }
    }
}
