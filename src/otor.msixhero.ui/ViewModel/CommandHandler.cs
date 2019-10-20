using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Input;
using MSI_Hero.Commands.RoutedCommand;
using MSI_Hero.Modules.Installed;
using MSI_Hero.Modules.Installed.Events;
using MSI_Hero.Modules.Installed.ViewModel;
using MSI_Hero.Modules.Settings;
using otor.msihero.lib;
using Prism.Events;
using Prism.Regions;
using Prism.Services.Dialogs;

namespace MSI_Hero.ViewModel
{
    public class CommandHandler
    {
        private readonly IEventAggregator eventAggregator;
        private readonly IRegionManager regionManager;
        private readonly IDialogService dialogService;

        public CommandHandler(IEventAggregator eventAggregator, IRegionManager regionManager, IDialogService dialogService)
        {
            this.eventAggregator = eventAggregator;
            this.regionManager = regionManager;
            this.dialogService = dialogService;

            this.OpenExplorer = new DelegateCommand(param => this.OpenExplorerExecute(param as PackageViewModel), param => this.CanOpenExplorer(param as PackageViewModel));
            this.OpenExplorerUser = new DelegateCommand(param => this.OpenExplorerUserExecute(param as PackageViewModel), param => this.CanOpenExplorerUser(param as PackageViewModel));
            this.OpenManifest = new DelegateCommand(param => this.OpenManifestExecute(param as PackageViewModel), param => this.CanOpenManifest(param as PackageViewModel));
            this.RunApp = new DelegateCommand(param => this.RunAppExecute(param as PackageViewModel), param => this.CanRunApp(param as PackageViewModel));
            // this.RunTool = new DelegateCommand(param => this.RunToolExecute(packageList.SelectedPackage, param as ToolViewModel), param => this.CanRunTool(packageList.SelectedPackage, param as ToolViewModel));
            this.OpenPowerShell = new DelegateCommand(this.OpenPowerShellExecute, this.CanOpenPowerShell);

            this.Refresh = new DelegateCommand(this.RefreshExecute, this.CanRefresh);
        }

        public ICommand Refresh { get; }

        public ICommand OpenPowerShell { get; }

        public ICommand OpenExplorer { get; }

        public ICommand OpenExplorerUser { get; }

        public ICommand OpenManifest { get; }

        public ICommand RunApp { get; }

        public ICommand RunTool { get; }

        private void RefreshExecute(object obj)
        {
            this.eventAggregator.GetEvent<InstalledListRefreshRequestEvent>().Publish(true);
        }

        private bool CanRefresh(object obj)
        {
            var hasRegion = this.regionManager.Regions.ContainsRegionWithName(InstalledModule.Path);
            if (!hasRegion)
            {
                return false;
            }

            var region = this.regionManager.Regions[InstalledModule.Path];
            return true;
        }

        private void OpenPowerShellExecute(object obj)
        {
            this.dialogService.ShowDialog(SettingsModule.Path, new DialogParameters(), c =>
            {

            });

            var process = new ProcessStartInfo("powershell.exe", "-NoExit -NoLogo -Command \"Import-Module Appx; Write-Host \"Module [Appx] has been automatically imported by MSIX Hero.\"");
            Process.Start(process);
        }

        private bool CanOpenPowerShell(object obj)
        {
            return true;
        }

        private void OpenExplorerExecute(PackageViewModel package)
        {
            if (package == null)
            {
                return;
            }

            System.Diagnostics.Process.Start("explorer.exe", "/e," + package.InstallLocation);
        }

        private bool CanOpenExplorer(PackageViewModel package)
        {
            return package != null;
        }

        private void OpenExplorerUserExecute(PackageViewModel package)
        {
            if (package == null)
            {
                return;
            }

            System.Diagnostics.Process.Start("explorer.exe", "/e," + package.UserDataPath);
        }

        private bool CanOpenExplorerUser(PackageViewModel package)
        {
            return package != null;
        }

        private void OpenManifestExecute(PackageViewModel package)
        {
            if (package == null)
            {
                return;
            }

            var spi = new ProcessStartInfo(package.ManifestLocation) { UseShellExecute = true };
            Process.Start(spi);
        }

        private bool CanOpenManifest(PackageViewModel package)
        {
            return package != null;
        }

        private void RunAppExecute(PackageViewModel package)
        {
            if (package == null)
            {
                return;
            }

            //this.packageList.PackageManager.RunApp((Package)package);
        }

        private bool CanRunApp(PackageViewModel package)
        {
            return package != null;
        }

        private void RunToolExecute(PackageViewModel package, ToolViewModel tool)
        {
            if (package == null || tool == null)
            {
                return;
            }

            //this.packageList.PackageManager.RunTool((Package)package, tool.Name);
        }

        private bool CanRunTool(PackageViewModel package, ToolViewModel tool)
        {
            return package != null && tool != null;
        }
    }
}
