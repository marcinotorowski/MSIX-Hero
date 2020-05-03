using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Appx.Manifest.Summary;
using otor.msixhero.lib.Domain.Appx.Packages;
using otor.msixhero.lib.Domain.Commands.Packages.Developer;
using otor.msixhero.lib.Domain.Commands.Packages.Grid;
using otor.msixhero.lib.Domain.Commands.Packages.Manager;
using otor.msixhero.lib.Domain.Commands.Packages.Signing;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Configuration;
using otor.msixhero.lib.Infrastructure.Helpers;
using otor.msixhero.lib.Infrastructure.Progress;
using otor.msixhero.ui.Commands.RoutedCommand;
using otor.msixhero.ui.Modules.Dialogs.PackageExpert.ViewModel;
using Prism.Services.Dialogs;

namespace otor.msixhero.ui.Modules.PackageList.ViewModel
{
    public enum AppInstallerCommandParameter
    {
        Empty,
        Selection,
        Browse
    }

    public enum PackageExpertCommandParameter
    {
        Selection,
        Browse
    }

    public class PackageListCommandHandler
    {
        private readonly IInteractionService interactionService;
        private readonly IApplicationStateManager stateManager;
        private readonly IConfigurationService configurationService;
        private readonly IDialogService dialogService;
        private readonly IBusyManager busyManager;
        private readonly FileInvoker fileInvoker;

        public PackageListCommandHandler(
            IInteractionService interactionService,
            IConfigurationService configurationService,
            IApplicationStateManager stateManager, 
            IDialogService dialogService,
            IBusyManager busyManager)
        {
            this.interactionService = interactionService;
            this.configurationService = configurationService;
            this.stateManager = stateManager;
            this.dialogService = dialogService;
            this.busyManager = busyManager;
            this.fileInvoker = new FileInvoker(interactionService);

            // Package-specific
            this.OpenExplorer = new DelegateCommand(param => this.OpenExplorerExecute(), param => this.CanExecuteSingleSelectionOnManifest());
            this.Deprovision = new DelegateCommand(param => this.DeprovisionExecute(), param => this.CanExecuteDeprovision());
            this.OpenExplorerUser = new DelegateCommand(param => this.OpenExplorerUserExecute(), param => this.CanExecuteOpenExplorerUser());
            this.OpenManifest = new DelegateCommand(param => this.OpenManifestExecute(), param => this.CanExecuteOpenManifest());
            this.OpenConfigJson = new DelegateCommand(param => this.OpenConfigJsonExecute(), param => this.CanExecuteOpenConfigJson());
            this.RunApp = new DelegateCommand(param => this.RunAppExecute(), param => this.CanExecuteSingleSelectionOnManifest());
            this.RunTool = new DelegateCommand(param => this.RunToolExecute(param as ToolListConfiguration), param => this.CanRunTool(param as ToolListConfiguration));
            this.OpenPowerShell = new DelegateCommand(param => this.OpenPowerShellExecute(), param => this.CanExecuteOpenPowerShell());
            this.RemovePackage = new DelegateCommand(param => this.RemovePackageExecute(param is bool &&(bool)param), param => this.CanExecuteSingleSelection());
            this.MountRegistry = new DelegateCommand(param => this.MountRegistryExecute(), param => this.CanExecuteMountRegistry());
            this.DismountRegistry = new DelegateCommand(param => this.DismountRegistryExecute(), param => this.CanExecuteDismountRegistry());

            // General APPX
            this.AddPackage = new DelegateCommand(param => this.AddPackageExecute(param is bool boolParam && boolParam), param => this.CanExecuteAddPackage());
            this.OpenLogs = new DelegateCommand(param => this.OpenLogsExecute(), param => true);
            this.Pack = new DelegateCommand(param => this.PackExecute());
            this.AppAttach = new DelegateCommand(param => this.AppAttachExecute(param is bool && (bool)param), param => this.CanExecuteSingleSelection(param is bool && (bool)param));
            this.AppInstaller = new DelegateCommand(param => this.AppInstallerExecute(param is AppInstallerCommandParameter parameter ? parameter : AppInstallerCommandParameter.Empty), param => this.CanExecuteAppInstaller(param is AppInstallerCommandParameter parameter ? parameter : AppInstallerCommandParameter.Empty));
            this.PackageExpert = new DelegateCommand(param => this.PackageExpertExecute(), param => this.CanExecutePackageExpert());
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
        }

        public ICommand Refresh { get; }

        public ICommand OpenPowerShell { get; }

        public ICommand DismountRegistry { get; }

        public ICommand RemovePackage { get; }

        public ICommand NewSelfSignedCert { get; }

        public ICommand ExtractCert { get; }

        public ICommand InstallCertificate { get; }

        public ICommand OpenLogs { get; }
        
        public ICommand Pack { get; }
        
        public ICommand AppInstaller { get; }

        public ICommand PackageExpert { get; }

        public ICommand AppAttach { get; }

        public ICommand ModificationPackage { get; }

        public ICommand Unpack { get; }
        
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

        private async void RefreshExecute()
        {
            var context = this.busyManager.Begin(OperationType.PackageLoading);
            try
            {
                await this.stateManager.CommandExecutor.GetExecuteAsync(new GetPackages(this.stateManager.CurrentState.Packages.Context), CancellationToken.None, context).ConfigureAwait(false);
            }
            finally
            {
                this.busyManager.End(context);
            }
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

        private async void AddPackageExecute(bool forAllUsers)
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
            
            var context = this.busyManager.Begin();
            try
            {
                using (var wrappedProgress = new WrappedProgress(context))
                {
                    var p1 = wrappedProgress.GetChildProgress(90);
                    var p2 = wrappedProgress.GetChildProgress(10);
                    await this.stateManager.CommandExecutor.ExecuteAsync(command, CancellationToken.None, p1).ConfigureAwait(false);

                    AppxManifestSummary appxReader;

                    var allPackages = await this.stateManager.CommandExecutor.GetExecuteAsync(new GetPackages(this.stateManager.CurrentState.Packages.Context), CancellationToken.None, p2).ConfigureAwait(false);
                    if (!string.Equals(".appinstaller", Path.GetExtension(selection), StringComparison.OrdinalIgnoreCase))
                    {
                        appxReader = await AppxManifestSummaryBuilder.FromFile(selection, AppxManifestSummaryBuilderMode.Identity).ConfigureAwait(false);
                    }
                    else
                    {
                        appxReader = null;
                    }

                    if (appxReader != null)
                    {
                        var selected = allPackages.FirstOrDefault(p => p.Name == appxReader.Name);
                        await this.stateManager.CommandExecutor.ExecuteAsync(new SelectPackages(selected) { IsExplicit = true }).ConfigureAwait(false);
                    }
                }
            }
            finally
            {
                this.busyManager.End(context);
            }
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
        private async void DeprovisionExecute()
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

            var context = this.busyManager.Begin();
            try
            {
                var command = new Deprovision(selection.First().PackageFamilyName);
                using (var pw = new WrappedProgress(context))
                {
                    var p1 = pw.GetChildProgress(80);
                    var p2 = pw.GetChildProgress(20);

                    await this.stateManager.CommandExecutor.ExecuteAsync(command, CancellationToken.None, p1).ConfigureAwait(false);
                    await this.stateManager.CommandExecutor.ExecuteAsync(SelectPackages.CreateEmpty(), CancellationToken.None).ConfigureAwait(false);
                    await this.stateManager.CommandExecutor.ExecuteAsync(new GetPackages(this.stateManager.CurrentState.Packages.Context), CancellationToken.None, p2).ConfigureAwait(false);
                }
            }
            finally
            {
                this.busyManager.End(context);
            }
        }

        private void DismountRegistryExecute()
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

            this.stateManager.CommandExecutor.ExecuteAsync(new DismountRegistry(selection.First()));
        }
        
        private bool CanExecuteMountRegistry()
        {
            var selection = this.stateManager.CurrentState.Packages.SelectedItems;
            if (selection.Count != 1)
            {
                return false;
            }

            var selected = selection.First();
            if (selected?.InstallLocation == null)
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

        private bool CanExecuteDismountRegistry()
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
            this.fileInvoker.Execute(config.ManifestEditorType, config.ManifestEditor, package.ManifestLocation);
        }

        private void OpenConfigJsonExecute()
        {
            var package = this.stateManager.CurrentState.Packages.SelectedItems.FirstOrDefault();
            if (package == null)
            {
                return;
            }

            var config = this.configurationService.GetCurrentConfiguration().Editing ?? new EditingConfiguration();
            this.fileInvoker.Execute(config.PsfEditorType, config.PsfEditor, package.PsfConfig);
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

            var context = this.busyManager.Begin();
            try
            {
                await this.stateManager.CommandExecutor.ExecuteAsync(new RemovePackages(allUsersRemoval ? PackageContext.AllUsers : PackageContext.CurrentUser, selection), CancellationToken.None, context).ConfigureAwait(false);
            }
            finally
            {
                this.busyManager.End(context);
            }
        }

        private async void RunToolExecute(ToolListConfiguration tool)
        {
            var package = this.stateManager.CurrentState.Packages.SelectedItems.FirstOrDefault();
            if (package == null || tool == null)
            {
                return;
            }

            var command = new RunToolInPackage(package, tool.Path, tool.Arguments) {AsAdmin = tool.AsAdmin};
            var context = this.busyManager.Begin();
            try
            {
                await this.stateManager.CommandExecutor.ExecuteAsync(command, CancellationToken.None, context).ConfigureAwait(false);
            }
            finally
            {
                this.busyManager.End(context);
            }
        }

        private bool CanRunTool(ToolListConfiguration tool)
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
            this.dialogService.ShowDialog(Constants.PathNewSelfSigned, new DialogParameters(), this.OnDialogClosed);
        }

        private void ExtractCertExecute()
        {
            this.dialogService.ShowDialog(Constants.PathCertificateExport, new DialogParameters(), this.OnDialogClosed);
        }

        private void OpenLogsExecute()
        {
            // this.dialogService.ShowDialog(Constants.PathPsfExpert, new DialogParameters(), this.OnDialogClosed);
            this.dialogService.ShowDialog(Constants.PathEventViewer, new DialogParameters(), this.OnDialogClosed);
        }

        private void UnpackExecute()
        {
            this.dialogService.ShowDialog(Constants.PathUnpack, new DialogParameters(), this.OnDialogClosed);
        }

        private void PackExecute()
        {
            this.dialogService.ShowDialog(Constants.PathPack, new DialogParameters(), this.OnDialogClosed);
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
        private bool CanExecutePackageExpert()
        {
            return this.stateManager.CurrentState.Packages.SelectedItems.Count == 1;
        }

        private void PackageExpertExecute()
        {
            if (this.stateManager.CurrentState.Packages.SelectedItems.Count != 1)
            {
                return;
            }

            var parameters = new PackageExpertSelection(this.stateManager.CurrentState.Packages.SelectedItems.First().ManifestLocation).ToDialogParameters();
            this.dialogService.ShowDialog(Constants.PathPackageExpert, parameters, this.OnDialogClosed);
        }

        private void AppInstallerExecute(AppInstallerCommandParameter parameter)
        {
            switch (parameter)
            {
                case AppInstallerCommandParameter.Empty:
                    this.dialogService.ShowDialog(Constants.PathAppInstaller, new DialogParameters(), this.OnDialogClosed);
                    break;
                case AppInstallerCommandParameter.Selection:
                    if (this.stateManager.CurrentState.Packages.SelectedItems.Count != 1)
                    {
                        this.dialogService.ShowDialog(Constants.PathAppInstaller, new DialogParameters(), this.OnDialogClosed);
                    }
                    else
                    {
                        var parameters = new DialogParameters
                        {
                            { "file", this.stateManager.CurrentState.Packages.SelectedItems.First().ManifestLocation }
                        };

                        this.dialogService.ShowDialog(Constants.PathAppInstaller, parameters, this.OnDialogClosed);
                    }
                    break;
                case AppInstallerCommandParameter.Browse:
                    if (this.interactionService.SelectFile("Appinstaller files|*.appinstaller|All files|*.*", out var selected))
                    {
                        var parameters = new DialogParameters
                        {
                            { "file", selected }
                        };

                        this.dialogService.ShowDialog(Constants.PathAppInstaller, parameters, this.OnDialogClosed);
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
                this.dialogService.ShowDialog(Constants.PathModificationPackage, new DialogParameters(), this.OnDialogClosed);
            }
            else
            {
                var parameters = new DialogParameters
                {
                    { "file", this.stateManager.CurrentState.Packages.SelectedItems.First().ManifestLocation }
                };

                this.dialogService.ShowDialog(Constants.PathModificationPackage, parameters, this.OnDialogClosed);
            }
        }

        private void AppAttachExecute(bool forSelection)
        {
            if (!forSelection || this.stateManager.CurrentState.Packages.SelectedItems.Count != 1)
            {
                this.dialogService.ShowDialog(Constants.PathAppAttach, new DialogParameters(), this.OnDialogClosed);
            }
            else
            {
                var parameters = new DialogParameters
                {
                    { "file", this.stateManager.CurrentState.Packages.SelectedItems.First().ManifestLocation }
                };

                this.dialogService.ShowDialog(Constants.PathAppAttach, parameters, this.OnDialogClosed);
            }
        }

        private void OpenResignExecute()
        {
            this.dialogService.ShowDialog(Constants.PathPackageSigning, new DialogParameters(), this.OnDialogClosed);
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
    }
}
