using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using otor.msixhero.lib;
using otor.msixhero.lib.BusinessLayer.Commands;
using otor.msixhero.lib.BusinessLayer.Commands.Developer;
using otor.msixhero.lib.BusinessLayer.Commands.Grid;
using otor.msixhero.lib.BusinessLayer.Commands.Manager;
using otor.msixhero.lib.BusinessLayer.Commands.Signing;
using otor.msixhero.lib.BusinessLayer.Infrastructure;
using otor.msixhero.lib.BusinessLayer.Models.Packages;
using otor.msixhero.lib.BusinessLayer.State.Enums;
using otor.msixhero.lib.Ipc;
using otor.msixhero.lib.Managers;
using otor.msixhero.lib.Services;
using otor.msixhero.ui.Commands.RoutedCommand;
using otor.msixhero.ui.Modules.Dialogs;
using otor.msixhero.ui.Services;
using Prism.Regions;
using Prism.Services.Dialogs;

namespace otor.msixhero.ui.ViewModel
{
    public class CommandHandler
    {
        private readonly IInteractionService interactionService;
        private readonly IApplicationStateManager stateManager;
        private readonly IAppxPackageManager packageManager;
        private readonly IRegionManager regionManager;
        private readonly IDialogService dialogService;

        public CommandHandler(
            IInteractionService interactionService,
            IApplicationStateManager stateManager, 
            IAppxPackageManager packageManager,
            IRegionManager regionManager, 
            IDialogService dialogService)
        {
            this.interactionService = interactionService;
            this.stateManager = stateManager;
            this.packageManager = packageManager;
            this.regionManager = regionManager;
            this.dialogService = dialogService;

            this.OpenExplorer = new DelegateCommand(param => this.OpenExplorerExecute(), param => this.CanOpenExplorer());
            this.OpenExplorerUser = new DelegateCommand(param => this.OpenExplorerUserExecute(), param => this.CanOpenExplorerUser());
            this.OpenManifest = new DelegateCommand(param => this.OpenManifestExecute(), param => this.CanOpenManifest());
            this.RunApp = new DelegateCommand(param => this.RunAppExecute(), param => this.CanRunApp());
            this.RunTool = new DelegateCommand(param => this.RunToolExecute(param as string), param => this.CanRunTool(param as string));
            this.OpenPowerShell = new DelegateCommand(param => this.OpenPowerShellExecute(), param => this.CanOpenPowerShell());
            this.RemovePackage = new DelegateCommand(param => this.RemovePackageExecute(param is bool &&(bool)param), param => this.CanRemovePackage());

            this.MountRegistry = new DelegateCommand(param => this.MountRegistryExecute(), param => this.CanMountRegistry());
            this.UnmountRegistry = new DelegateCommand(param => this.UnmountRegistryExecute(), param => this.CanUnmountRegistry());

            this.Refresh = new DelegateCommand(param => this.RefreshExecute(), param => this.CanRefresh());
            this.NewSelfSignedCert = new DelegateCommand(param => this.NewSelfSignedCertExecute(), param => true);
            this.OpenLogs = new DelegateCommand(param => this.OpenLogsExecute(), param => true);
            this.AddPackage = new DelegateCommand(param => this.AddPackageExecute(), param => this.CanAddPackage());
            this.OpenAppsFeatures = new DelegateCommand(param => this.OpenAppsFeaturesExecute());
            this.OpenDevSettings = new DelegateCommand(param => this.OpenDevSettingsExecute());
            this.InstallCertificate = new DelegateCommand(param => this.InstallCertificateExecute());
        }

        public ICommand Refresh { get; }

        public ICommand OpenPowerShell { get; }

        public ICommand UnmountRegistry { get; }

        public ICommand RemovePackage { get; }

        public ICommand NewSelfSignedCert { get; }

        public ICommand InstallCertificate { get; }

        public ICommand OpenLogs { get; }

        public ICommand MountRegistry { get; }

        public ICommand OpenExplorer { get; }

        public ICommand OpenExplorerUser { get; }

        public ICommand OpenManifest { get; }

        public ICommand RunApp { get; }

        public ICommand RunTool { get; }

        public ICommand AddPackage { get; }

        public ICommand OpenAppsFeatures { get; }

        public ICommand OpenDevSettings { get; }

        private void RefreshExecute()
        {
            this.stateManager.CommandExecutor.ExecuteAsync(new GetPackages(this.stateManager.CurrentState.Packages.Context));
        }

        private bool CanRefresh()
        {
            // todo: refresh only when region activated
            return true;
        }

        private bool CanAddPackage()
        {
            return true;
        }

        private void AddPackageExecute()
        {
            if (!this.interactionService.SelectFile("MSIX packages (*.msix)|*.msix", out var selection))
            {
                return;
            }


            this.stateManager.CommandExecutor.ExecuteAsync(new AddPackage(selection), CancellationToken.None);
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

        private void UnmountRegistryExecute()
        {
            var selection = this.stateManager.CurrentState.Packages.SelectedItems;
            if (selection.Count != 1)
            {
                return;
            }

            this.stateManager.CommandExecutor.ExecuteAsync(new UnmountRegistry(selection.First()));
        }

        private bool CanMountRegistry()
        {
            var selection = this.stateManager.CurrentState.Packages.SelectedItems;
            if (selection.Count != 1)
            {
                return false;
            }

            return this.packageManager.GetRegistryMountState(selection.First()).Result == RegistryMountState.NotMounted;
        }

        private bool CanUnmountRegistry()
        {
            var selection = this.stateManager.CurrentState.Packages.SelectedItems;
            if (selection.Count != 1)
            {
                return false;
            }

            return this.packageManager.GetRegistryMountState(selection.First()).Result == RegistryMountState.Mounted;
        }

        private bool CanOpenPowerShell()
        {
            return true;
        }
        
        private bool CanRemovePackage()
        {
            var package = this.stateManager.CurrentState.Packages.SelectedItems.FirstOrDefault();
            return package != null;
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

        private bool CanOpenExplorer()
        {
            var package = this.stateManager.CurrentState.Packages.SelectedItems.FirstOrDefault();
            return package != null;
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

        private bool CanOpenExplorerUser()
        {
            var package = this.stateManager.CurrentState.Packages.SelectedItems.FirstOrDefault();
            return package != null;
        }

        private void OpenManifestExecute()
        {
            var package = this.stateManager.CurrentState.Packages.SelectedItems.FirstOrDefault();
            if (package == null)
            {
                return;
            }

            var spi = new ProcessStartInfo(package.ManifestLocation) { UseShellExecute = true };
            Process.Start(spi);
        }

        private bool CanOpenManifest()
        {
            var package = this.stateManager.CurrentState.Packages.SelectedItems.FirstOrDefault();
            return package != null;
        }

        private void RunAppExecute()
        {
            var package = this.stateManager.CurrentState.Packages.SelectedItems.FirstOrDefault();
            if (package == null)
            {
                return;
            }

            this.packageManager.Run(package);
        }

        private void RemovePackageExecute(bool allUsersRemoval)
        {
            var selection = this.stateManager.CurrentState.Packages.SelectedItems;
            if (!selection.Any())
            {
                return;
            }

            this.stateManager.CommandExecutor.ExecuteAsync(new RemovePackages(allUsersRemoval ? PackageContext.AllUsers : PackageContext.CurrentUser, selection));
        }

        private bool CanRunApp()
        {
            var package = this.stateManager.CurrentState.Packages.SelectedItems.FirstOrDefault();
            return package != null;
        }

        private void RunToolExecute(string tool)
        {
            var package = this.stateManager.CurrentState.Packages.SelectedItems.FirstOrDefault();
            if (package == null || tool == null)
            {
                return;
            }

            this.packageManager.RunToolInContext(package, tool);
        }

        private bool CanRunTool(string tool)
        {
            if (this.stateManager.CurrentState.Packages.SelectedItems.Count != 1)
            {
                return false;
            }

            return tool != null;
        }

        private void InstallCertificateExecute()
        {
            if (!this.interactionService.SelectFile("*.cer|Certificate files (*.cer)", out var selectedFile))
            {
                return;
            }

            this.stateManager.CommandExecutor.ExecuteAsync(new InstallCertificate(selectedFile));
        }

        private void NewSelfSignedCertExecute()
        {
            this.dialogService.ShowDialog(DialogsModule.NewSelfSignedPath, new DialogParameters(), this.OnSelfSignedDialogClosed);
        }

        private void OpenLogsExecute()
        {
            this.dialogService.ShowDialog(DialogsModule.EventViewerPath, new DialogParameters(), this.OnSelfSignedDialogClosed);
        }

        private void OnSelfSignedDialogClosed(IDialogResult obj)
        {
        }
    }
}
