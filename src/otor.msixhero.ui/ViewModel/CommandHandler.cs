using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Appx.Packages;
using otor.msixhero.lib.Domain.Commands.Developer;
using otor.msixhero.lib.Domain.Commands.Grid;
using otor.msixhero.lib.Domain.Commands.Manager;
using otor.msixhero.lib.Domain.Commands.Signing;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Configuration;
using otor.msixhero.ui.Commands.RoutedCommand;
using otor.msixhero.ui.Modules.Dialogs;
using otor.msixhero.ui.Modules.Settings;
using Prism.Services.Dialogs;

namespace otor.msixhero.ui.ViewModel
{
    public enum AppInstallerCommandParameter
    {
        Empty,
        Selection,
        Browse
    }

    public class CommandHandler
    {
        private readonly IInteractionService interactionService;
        private readonly IApplicationStateManager stateManager;
        private readonly IConfigurationService configurationService;
        private readonly IDialogService dialogService;

        public CommandHandler(
            IInteractionService interactionService,
            IConfigurationService configurationService,
            IApplicationStateManager stateManager, 
            IDialogService dialogService)
        {
            this.interactionService = interactionService;
            this.configurationService = configurationService;
            this.stateManager = stateManager;
            this.dialogService = dialogService;

            // Package-specific
            this.OpenExplorer = new DelegateCommand(param => this.OpenExplorerExecute(), param => this.CanExecuteSingleSelectionOnManifest());
            this.Deprovision = new DelegateCommand(param => this.DeprovisionExecute(), param => this.CanExecuteDeprovision());
            this.OpenExplorerUser = new DelegateCommand(param => this.OpenExplorerUserExecute(), param => this.CanExecuteOpenExplorerUser());
            this.OpenManifest = new DelegateCommand(param => this.OpenManifestExecute(), param => this.CanExecuteOpenManifest());
            this.OpenConfigJson = new DelegateCommand(param => this.OpenConfigJsonExecute(), param => this.CanExecuteOpenConfigJson());
            this.RunApp = new DelegateCommand(param => this.RunAppExecute(), param => this.CanExecuteSingleSelectionOnManifest());
            this.RunTool = new DelegateCommand(param => this.RunToolExecute(param as string), param => this.CanRunTool(param as string));
            this.OpenPowerShell = new DelegateCommand(param => this.OpenPowerShellExecute(), param => this.CanExecuteOpenPowerShell());
            this.RemovePackage = new DelegateCommand(param => this.RemovePackageExecute(param is bool &&(bool)param), param => this.CanExecuteSingleSelection());
            this.MountRegistry = new DelegateCommand(param => this.MountRegistryExecute(), param => this.CanExecuteMountRegistry());
            this.UnmountRegistry = new DelegateCommand(param => this.UnmountRegistryExecute(), param => this.CanExecuteUnmountRegistry());

            // General APPX
            this.AddPackage = new DelegateCommand(param => this.AddPackageExecute(param is bool boolParam && boolParam), param => this.CanExecuteAddPackage());
            this.OpenLogs = new DelegateCommand(param => this.OpenLogsExecute(), param => true);
            this.Pack = new DelegateCommand(param => this.PackExecute());
            this.AppInstaller = new DelegateCommand(param => this.AppInstallerExecute(param is AppInstallerCommandParameter parameter ? parameter : AppInstallerCommandParameter.Empty), param => this.CanExecuteAppInstaller(param is AppInstallerCommandParameter parameter ? parameter : AppInstallerCommandParameter.Empty));
            this.ModificationPackage = new DelegateCommand(param => this.ModificationPackageExecute(param is bool && (bool)param), param => this.CanExecuteSingleSelection(param is bool && (bool)param));
            this.Unpack = new DelegateCommand(param => this.UnpackExecute());

            // Certificates
            this.NewSelfSignedCert = new DelegateCommand(param => this.NewSelfSignedCertExecute(), param => true);
            this.ExtractCert = new DelegateCommand(param => this.ExtractCertExecute(), param => true);
            this.InstallCertificate = new DelegateCommand(param => this.InstallCertificateExecute());
            this.OpenResign = new DelegateCommand(param => this.OpenResignExecute());
            this.CertManager = new DelegateCommand(param => this.CertManagerExecute(param is bool ? (bool)param : false));
            
            // Links
            this.OpenAppsFeatures = new DelegateCommand(param => this.OpenAppsFeaturesExecute());
            this.OpenDevSettings = new DelegateCommand(param => this.OpenDevSettingsExecute());

            // Miscellaneous
            this.Copy = new DelegateCommand(param => this.CopyExecute(param == null ? PackageProperty.FullName : (PackageProperty)param));
            this.Refresh = new DelegateCommand(param => this.RefreshExecute(), param => this.CanRefresh());
            this.Settings = new DelegateCommand(param => this.SettingsExecute(), param => true);
        }

        public ICommand Refresh { get; }

        public ICommand OpenPowerShell { get; }

        public ICommand UnmountRegistry { get; }

        public ICommand RemovePackage { get; }

        public ICommand NewSelfSignedCert { get; }

        public ICommand ExtractCert { get; }

        public ICommand InstallCertificate { get; }

        public ICommand OpenLogs { get; }
        
        public ICommand Pack { get; }
        
        public ICommand AppInstaller { get; }

        public ICommand ModificationPackage { get; }

        public ICommand Unpack { get; }

        public ICommand Settings { get; }

        public ICommand OpenResign { get; }

        public ICommand CertManager { get; }

        public ICommand MountRegistry { get; }

        public ICommand OpenExplorer { get; }

        public ICommand Deprovision { get; }

        public ICommand OpenExplorerUser { get; }

        public ICommand OpenManifest { get; }

        public ICommand OpenConfigJson { get; }

        public ICommand RunApp { get; }

        public ICommand RunTool { get; }

        public ICommand AddPackage { get; }

        public ICommand OpenAppsFeatures { get; }

        public ICommand OpenDevSettings { get; }

        public ICommand Copy { get; }

        private void RefreshExecute()
        {
            this.stateManager.CommandExecutor.ExecuteAsync(new GetPackages(this.stateManager.CurrentState.Packages.Context));
        }

        private bool CanRefresh()
        {
            // todo: refresh only when region activated
            return true;
        }

        private bool CanExecuteAddPackage()
        {
            return true;
        }

        private void AddPackageExecute(bool forAllUsers)
        {
            string selection;
            if (forAllUsers)
            {
                if (!this.interactionService.SelectFile("All supported files|*.msix;*.appx|Packages|*.msix;*.appx", out selection))
                {
                    return;
                }
            }
            else
            {
                if (!this.interactionService.SelectFile("All supported files|*.msix;*.appx;*.appxbundle;*.appinstaller;AppxManifest.xml|Packages and bundles|*.msix;*.appx;*.appxbundle|App installer files|*.appinstaller|Manifest files|AppxManifest.xml", out selection))
                {
                    return;
                }
            }

            var command = new AddPackage(selection)
            {
                AllUsers = forAllUsers,
                KillRunningApps = true,
                AllowDowngrade = false
            };

            this.stateManager.CommandExecutor.ExecuteAsync(command, CancellationToken.None);
        }

        private void OpenPowerShellExecute()
        {
            var process = new ProcessStartInfo("powershell.exe", "-NoExit -NoLogo -Command \"Import-Module Appx; Write-Host \"Module [Appx] has been automatically imported by MSIX Hero.\"");
            Process.Start(process);
        }

        private void OpenAppsFeaturesExecute()
        {
            var process = new ProcessStartInfo("ms-settings:appsfeatures") { UseShellExecute = true };
            Process.Start(process);
        }

        private void OpenDevSettingsExecute()
        {
            var process = new ProcessStartInfo("ms-settings:developers") { UseShellExecute = true };
            Process.Start(process);
        }

        private void MountRegistryExecute()
        {
            var selection = this.stateManager.CurrentState.Packages.SelectedItems;
            if (selection.Count != 1)
            {
                return;
            }

            this.stateManager.CommandExecutor.ExecuteAsync(new MountRegistry(selection.First(), true));
        }
        private void DeprovisionExecute()
        {
            var selection = this.stateManager.CurrentState.Packages.SelectedItems;
            if (selection.Count != 1)
            {
                return;
            }

            var selected = selection.First();
            if (!selected.IsProvisioned)
            {
                return;
            }

            this.stateManager.CommandExecutor.ExecuteAsync(new Deprovision(selection.First().PackageFamilyName));
        }

        private void UnmountRegistryExecute()
        {
            var selection = this.stateManager.CurrentState.Packages.SelectedItems;
            if (selection.Count != 1)
            {
                return;
            }

            if (selection.First().InstallLocation == null)
            {
                return;
            }

            this.stateManager.CommandExecutor.ExecuteAsync(new UnmountRegistry(selection.First()));
        }
        
        private bool CanExecuteMountRegistry()
        {
            var selection = this.stateManager.CurrentState.Packages.SelectedItems;
            if (selection.Count != 1)
            {
                return false;
            }

            var selected = selection.First();
            if (selected.InstallLocation == null)
            {
                return false;
            }

            try
            {
                var regState = this.stateManager.CommandExecutor.GetExecute(new GetRegistryMountState(selected.InstallLocation, selected.Name));
                return regState == RegistryMountState.NotMounted;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool CanExecuteUnmountRegistry()
        {
            var selection = this.stateManager.CurrentState.Packages.SelectedItems;
            if (selection.Count != 1)
            {
                return false;
            }

            var selected = selection.First();
            if (selected.InstallLocation == null)
            {
                return false;
            }

            try
            {
                var regState = this.stateManager.CommandExecutor.GetExecute(new GetRegistryMountState(selected.InstallLocation, selected.Name));
                return regState == RegistryMountState.Mounted;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool CanExecuteOpenPowerShell()
        {
            return true;
        }
        
        private bool CanExecuteSingleSelection()
        {
            var package = this.stateManager.CurrentState.Packages.SelectedItems.FirstOrDefault();
            return package != null;
        }
        
        private bool CanExecuteSingleSelectionOnManifest()
        {
            var package = this.stateManager.CurrentState.Packages.SelectedItems.FirstOrDefault();
            return package?.InstallLocation != null;
        }
        
        private bool CanExecuteDeprovision()
        {
            var package = this.stateManager.CurrentState.Packages.SelectedItems.FirstOrDefault();
            return package != null && package.IsProvisioned;
        }

        private void OpenExplorerExecute()
        {
            var package = this.stateManager.CurrentState.Packages.SelectedItems.FirstOrDefault();
            if (package == null)
            {
                return;
            }

            Process.Start("explorer.exe", "/e," + package.InstallLocation);
        }
        
        private void OpenExplorerUserExecute()
        {
            var package = this.stateManager.CurrentState.Packages.SelectedItems.FirstOrDefault();
            if (package == null)
            {
                return;
            }

            Process.Start("explorer.exe", "/e," + package.UserDataPath);
        }

        private bool CanExecuteOpenExplorerUser()
        {
            var package = this.stateManager.CurrentState.Packages.SelectedItems.FirstOrDefault();
            return package?.InstallLocation != null;
        }

        private void OpenManifestExecute()
        {
            var package = this.stateManager.CurrentState.Packages.SelectedItems.FirstOrDefault();
            if (package == null)
            {
                return;
            }

            var config = this.configurationService.GetCurrentConfiguration().Editing ?? new EditingConfiguration();
            switch (config.ManifestEditorType)
            {
                case EditorType.Default:
                {
                    var spi = new ProcessStartInfo(package.ManifestLocation) { UseShellExecute = true };
                    Process.Start(spi);
                    break;
                }

                case EditorType.Ask:
                {
                    Process.Start("rundll32.exe", "shell32.dll, OpenAs_RunDLL " + package.ManifestLocation);
                    break;
                }

                default:
                {
                    var spi = new ProcessStartInfo(config.ManifestEditor.Resolved, "\"" + package.ManifestLocation + "\"") { UseShellExecute = true };
                    Process.Start(spi);
                    break;
                }
            }
        }

        private void OpenConfigJsonExecute()
        {
            var package = this.stateManager.CurrentState.Packages.SelectedItems.FirstOrDefault();
            if (package == null)
            {
                return;
            }

            var config = this.configurationService.GetCurrentConfiguration().Editing ?? new EditingConfiguration();
            switch (config.PsfEditorType)
            {
                case EditorType.Default:
                {
                    var spi = new ProcessStartInfo(package.PsfConfig) { UseShellExecute = true };
                    Process.Start(spi);
                    break;
                }

                case EditorType.Ask:
                {
                    Process.Start("rundll32.exe", "shell32.dll, OpenAs_RunDLL " + package.PsfConfig);
                    break;
                }

                default:
                {
                    var spi = new ProcessStartInfo(config.PsfEditor.Resolved, "\"" + package.PsfConfig + "\"") { UseShellExecute = true };
                    Process.Start(spi);
                    break;
                }
            }
        }

        private bool CanExecuteOpenManifest()
        {
            var package = this.stateManager.CurrentState.Packages.SelectedItems.FirstOrDefault();
            return package?.InstallLocation != null;
        }

        private bool CanExecuteOpenConfigJson()
        {
            var package = this.stateManager.CurrentState.Packages.SelectedItems.FirstOrDefault();
            return package?.InstallLocation != null && File.Exists(package.PsfConfig);
        }

        private void RunAppExecute()
        {
            var package = this.stateManager.CurrentState.Packages.SelectedItems.FirstOrDefault();
            if (package == null)
            {
                return;
            }

            this.stateManager.CommandExecutor.ExecuteAsync(new RunPackage(package.PackageFamilyName, package.ManifestLocation));
        }

        private async void RemovePackageExecute(bool allUsersRemoval)
        {
            var selection = this.stateManager.CurrentState.Packages.SelectedItems;
            if (!selection.Any())
            {
                return;
            }

            await this.stateManager.CommandExecutor.ExecuteAsync(new RemovePackages(allUsersRemoval ? PackageContext.AllUsers : PackageContext.CurrentUser, selection)).ConfigureAwait(false);
        }

        private async void RunToolExecute(string tool)
        {
            var package = this.stateManager.CurrentState.Packages.SelectedItems.FirstOrDefault();
            if (package == null || tool == null)
            {
                return;
            }

            await this.stateManager.CommandExecutor.ExecuteAsync(new RunToolInPackage(package, tool)).ConfigureAwait(false);
        }

        private bool CanRunTool(string tool)
        {
            if (this.stateManager.CurrentState.Packages.SelectedItems.Count != 1)
            {
                return false;
            }

            return tool != null;
        }

        private async void InstallCertificateExecute()
        {
            if (!this.interactionService.SelectFile("Certificate files (*.cer)|*.cer|All files (*.*)|*.*", out var selectedFile))
            {
                return;
            }

            await this.stateManager.CommandExecutor.ExecuteAsync(new InstallCertificate(selectedFile)).ConfigureAwait(false);
        }

        private void NewSelfSignedCertExecute()
        {
            this.dialogService.ShowDialog(DialogsModule.NewSelfSignedPath, new DialogParameters(), this.OnDialogClosed);
        }

        private void ExtractCertExecute()
        {
            this.dialogService.ShowDialog(DialogsModule.CertificateExportPath, new DialogParameters(), this.OnDialogClosed);
        }

        private void OpenLogsExecute()
        {
            this.dialogService.ShowDialog(DialogsModule.EventViewerPath, new DialogParameters(), this.OnDialogClosed);
        }

        private void UnpackExecute()
        {
            this.dialogService.ShowDialog(DialogsModule.UnpackPath, new DialogParameters(), this.OnDialogClosed);
        }

        private void PackExecute()
        {
            this.dialogService.ShowDialog(DialogsModule.PackPath, new DialogParameters(), this.OnDialogClosed);
        }

        private bool CanExecuteSingleSelection(bool forSelection)
        {
            return !forSelection || this.stateManager.CurrentState.Packages.SelectedItems.Count == 1;
        }

        private bool CanExecuteAppInstaller(AppInstallerCommandParameter param)
        {
            switch (param)
            {
                case AppInstallerCommandParameter.Empty:
                case AppInstallerCommandParameter.Browse:
                    return true;
                case AppInstallerCommandParameter.Selection:
                    return this.stateManager.CurrentState.Packages.SelectedItems.Count == 1;
                default:
                    return false;
            }
        }

        private void AppInstallerExecute(AppInstallerCommandParameter parameter)
        {
            switch (parameter)
            {
                case AppInstallerCommandParameter.Empty:
                    this.dialogService.ShowDialog(DialogsModule.AppInstallerPath, new DialogParameters(), this.OnDialogClosed);
                    break;
                case AppInstallerCommandParameter.Selection:
                    if (this.stateManager.CurrentState.Packages.SelectedItems.Count != 1)
                    {
                        this.dialogService.ShowDialog(DialogsModule.AppInstallerPath, new DialogParameters(), this.OnDialogClosed);
                    }
                    else
                    {
                        var parameters = new DialogParameters
                        {
                            { "file", this.stateManager.CurrentState.Packages.SelectedItems.First().ManifestLocation }
                        };

                        this.dialogService.ShowDialog(DialogsModule.AppInstallerPath, parameters, this.OnDialogClosed);
                    }
                    break;
                case AppInstallerCommandParameter.Browse:
                    if (this.interactionService.SelectFile("Appinstaller files|*.appinstaller|All files|*.*", out var selected))
                    {
                        var parameters = new DialogParameters
                        {
                            { "file", selected }
                        };

                        this.dialogService.ShowDialog(DialogsModule.AppInstallerPath, parameters, this.OnDialogClosed);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(parameter), parameter, null);
            }
        }

        private void ModificationPackageExecute(bool forSelection)
        {
            if (!forSelection || this.stateManager.CurrentState.Packages.SelectedItems.Count != 1)
            {
                this.dialogService.ShowDialog(DialogsModule.ModificationPackagePath, new DialogParameters(), this.OnDialogClosed);
            }
            else
            {
                var parameters = new DialogParameters
                {
                    { "file", this.stateManager.CurrentState.Packages.SelectedItems.First().ManifestLocation }
                };

                this.dialogService.ShowDialog(DialogsModule.ModificationPackagePath, parameters, this.OnDialogClosed);
            }
        }

        private void OpenResignExecute()
        {
            this.dialogService.ShowDialog(DialogsModule.PackageSigningPath, new DialogParameters(), this.OnDialogClosed);
        }

        private void CertManagerExecute(bool perMachine)
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

        private void CopyExecute(PackageProperty param)
        {
            if (!this.stateManager.CurrentState.Packages.SelectedItems.Any())
            {
                return;
            }

            var sb = new StringBuilder();

            foreach (var item in this.stateManager.CurrentState.Packages.SelectedItems)
            {
                if (sb.Length > 0)
                {
                    sb.Append(Environment.NewLine);
                }

                switch (param)
                {
                    case PackageProperty.Name:
                        sb.Append(item.Name);
                        break;
                    case PackageProperty.DisplayName:
                        sb.Append(item.DisplayName);
                        break;
                    case PackageProperty.FullName:
                        sb.Append(item.PackageId);
                        break;
                    case PackageProperty.Version:
                        sb.Append(item.Version);
                        break;
                    case PackageProperty.Publisher:
                        sb.Append(item.DisplayPublisherName);
                        break;
                    case PackageProperty.Subject:
                        sb.Append(item.Publisher);
                        break;
                    case PackageProperty.InstallPath:
                        sb.Append(item.InstallLocation);
                        break;
                }
            }

            Clipboard.SetText(sb.ToString(), TextDataFormat.UnicodeText);
        }

        private void OnDialogClosed(IDialogResult obj)
        {
        }

        private void SettingsExecute()
        {
            this.dialogService.ShowDialog(SettingsModule.Path, new DialogParameters(), this.OnDialogClosed);
        }
    }
}
