using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using MSI_Hero.Commands.RoutedCommand;
using MSI_Hero.Domain;
using MSI_Hero.Domain.Actions;
using otor.msihero.lib;
using Prism.Regions;
using Prism.Services.Dialogs;

namespace MSI_Hero.ViewModel
{
    public class CommandHandler
    {
        private readonly IApplicationStateManager stateManager;
        private readonly IAppxPackageManager packageManager;
        private readonly IRegionManager regionManager;
        private readonly IDialogService dialogService;

        public CommandHandler(
            IApplicationStateManager stateManager, 
            IAppxPackageManager packageManager,
            IRegionManager regionManager, 
            IDialogService dialogService)
        {
            this.stateManager = stateManager;
            this.packageManager = packageManager;
            this.regionManager = regionManager;
            this.dialogService = dialogService;

            this.OpenExplorer = new DelegateCommand(param => this.OpenExplorerExecute(), param => this.CanOpenExplorer());
            this.OpenExplorerUser = new DelegateCommand(param => this.OpenExplorerUserExecute(), param => this.CanOpenExplorerUser());
            this.OpenManifest = new DelegateCommand(param => this.OpenManifestExecute(), param => this.CanOpenManifest());
            this.RunApp = new DelegateCommand(param => this.RunAppExecute(), param => this.CanRunApp());
            this.RunTool = new DelegateCommand(param => this.RunToolExecute(param as ToolViewModel), param => this.CanRunTool(param as ToolViewModel));
            this.OpenPowerShell = new DelegateCommand(param => this.OpenPowerShellExecute(), param => this.CanOpenPowerShell());

            this.MountRegistry = new DelegateCommand(param => this.MountRegistryExecute(), param => this.CanMountRegistry());
            this.UnmountRegistry = new DelegateCommand(param => this.UnmountRegistryExecute(), param => this.CanUnmountRegistry());

            this.Refresh = new DelegateCommand(param => this.RefreshExecute(), param => this.CanRefresh());
        }

        public ICommand Refresh { get; }

        public ICommand OpenPowerShell { get; }

        public ICommand UnmountRegistry { get; }

        public ICommand MountRegistry { get; }

        public ICommand OpenExplorer { get; }

        public ICommand OpenExplorerUser { get; }

        public ICommand OpenManifest { get; }

        public ICommand RunApp { get; }

        public ICommand RunTool { get; }

        private void RefreshExecute()
        {
            this.stateManager.Executor.ExecuteAsync(new ReloadPackages());
        }

        private bool CanRefresh()
        {
            // todo: refresh only when region activated
            return true;
        }

        private void OpenPowerShellExecute()
        {
            var process = new ProcessStartInfo("powershell.exe", "-NoExit -NoLogo -Command \"Import-Module Appx; Write-Host \"Module [Appx] has been automatically imported by MSIX Hero.\"");
            Process.Start(process);
        }

        private void MountRegistryExecute()
        {
            var selection = this.stateManager.CurrentState.Packages.SelectedItems;
            if (selection.Count != 1)
            {
                return;
            }

            this.packageManager.MountRegistry(selection.First(), true);
        }

        private void UnmountRegistryExecute()
        {
            var selection = this.stateManager.CurrentState.Packages.SelectedItems;
            if (selection.Count != 1)
            {
                return;
            }

            this.packageManager.UnmountRegistry(selection.First());
        }

        private bool CanMountRegistry()
        {
            var selection = this.stateManager.CurrentState.Packages.SelectedItems;
            if (selection.Count != 1)
            {
                return false;
            }

            return this.packageManager.GetRegistryMountState(selection.First()) == RegistryMountState.NotMounted;
        }

        private bool CanUnmountRegistry()
        {
            var selection = this.stateManager.CurrentState.Packages.SelectedItems;
            if (selection.Count != 1)
            {
                return false;
            }

            return this.packageManager.GetRegistryMountState(selection.First()) == RegistryMountState.Mounted;
        }

        private bool CanOpenPowerShell()
        {
            return true;
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

            this.packageManager.RunApp(package);
        }

        private bool CanRunApp()
        {
            var package = this.stateManager.CurrentState.Packages.SelectedItems.FirstOrDefault();
            return package != null;
        }

        private void RunToolExecute(ToolViewModel tool)
        {
            var package = this.stateManager.CurrentState.Packages.SelectedItems.FirstOrDefault();
            if (package == null || tool == null)
            {
                return;
            }

            this.packageManager.RunTool(package, tool.Name);
        }

        private bool CanRunTool(ToolViewModel tool)
        {
            if (this.stateManager.CurrentState.Packages.SelectedItems.Count != 1)
            {
                return false;
            }

            return tool != null;
        }
    }
}
