using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Otor.MsixHero.Appx.Diagnostic.Registry;
using Otor.MsixHero.Appx.Diagnostic.Registry.Enums;
using Otor.MsixHero.Appx.Packaging.Installation;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities.Summary;
using Otor.MsixHero.Appx.Signing;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Helpers;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation.Enums;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Lib.Infrastructure;
using Otor.MsixHero.Lib.Infrastructure.Progress;
using Otor.MsixHero.Ui.Commands.RoutedCommand;
using Otor.MsixHero.Ui.Hero;
using Otor.MsixHero.Ui.Hero.Commands;
using Otor.MsixHero.Ui.Hero.Commands.Base;
using Otor.MsixHero.Ui.Hero.Commands.Packages;
using Otor.MsixHero.Ui.Hero.Executor;
using Otor.MsixHero.Ui.Modules.Dialogs.PackageExpert.ViewModel;
using Prism.Services.Dialogs;

namespace Otor.MsixHero.Ui.Modules.PackageList.ViewModel
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
        private readonly ISelfElevationProxyProvider<IAppxPackageManager> packageManagerProvider;
        private readonly ISelfElevationProxyProvider<ISigningManager> signingManagerProvider;
        private readonly ISelfElevationProxyProvider<IRegistryManager> registryManagerProvider;
        private readonly IMsixHeroApplication application;
        private readonly IInteractionService interactionService;
        private readonly IConfigurationService configurationService;
        private readonly IDialogService dialogService;
        private readonly IBusyManager busyManager;
        private readonly FileInvoker fileInvoker;

        public PackageListCommandHandler(
            ISelfElevationProxyProvider<IAppxPackageManager> packageManagerProvider,
            ISelfElevationProxyProvider<ISigningManager> signingManagerProvider,
            ISelfElevationProxyProvider<IRegistryManager> registryManagerProvider,
            IMsixHeroApplication application,
            IInteractionService interactionService,
            IConfigurationService configurationService,
            IDialogService dialogService,
            IBusyManager busyManager)
        {
            this.packageManagerProvider = packageManagerProvider;
            this.signingManagerProvider = signingManagerProvider;
            this.registryManagerProvider = registryManagerProvider;
            this.application = application;
            this.interactionService = interactionService;
            this.configurationService = configurationService;
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
            this.StopApp = new DelegateCommand(param => this.StopAppExecute(), param => this.CanExecuteStopApp());
            this.RunTool = new DelegateCommand(param => this.RunToolExecute(param as ToolListConfiguration), param => this.CanRunTool(param as ToolListConfiguration));
            this.OpenPowerShell = new DelegateCommand(param => this.OpenPowerShellExecute(), param => this.CanExecuteOpenPowerShell());
            this.RemovePackage = new DelegateCommand(param => this.RemovePackageExecute(param is bool b && b), param => this.CanExecuteSingleSelection());
            this.MountRegistry = new DelegateCommand(param => this.MountRegistryExecute(), param => this.CanExecuteMountRegistry());
            this.DismountRegistry = new DelegateCommand(param => this.DismountRegistryExecute(), param => this.CanExecuteDismountRegistry());
            this.ChangeVolume = new DelegateCommand(param => this.ChangeVolumeExecute(), param => this.CanExecuteChangeVolume());

            // General APPX
            this.AddPackage = new DelegateCommand(param => this.AddPackageExecute(param is bool boolParam && boolParam), param => this.CanExecuteAddPackage());
            this.OpenLogs = new DelegateCommand(param => this.OpenLogsExecute(), param => true);
            this.Pack = new DelegateCommand(param => this.PackExecute());
            this.AppAttach = new DelegateCommand(param => this.AppAttachExecute(param is bool && (bool)param), param => this.CanExecuteSingleSelection(param is bool && (bool)param));
            this.AppInstaller = new DelegateCommand(param => this.AppInstallerExecute(param is AppInstallerCommandParameter parameter ? parameter : AppInstallerCommandParameter.Empty), param => this.CanExecuteAppInstaller(param is AppInstallerCommandParameter parameter ? parameter : AppInstallerCommandParameter.Empty));
            this.PackageExpert = new DelegateCommand(param => this.PackageExpertExecute(), param => this.CanExecutePackageExpert());
            this.ModificationPackage = new DelegateCommand(param => this.ModificationPackageExecute(param is bool && (bool)param), param => this.CanExecuteSingleSelection(param is bool && (bool)param));
            this.Unpack = new DelegateCommand(param => this.UnpackExecute());
            this.UpdateImpact = new DelegateCommand(param => this.UpdateImpactExecute());
            this.Winget = new DelegateCommand(param => this.WingetExecute(param is AppInstallerCommandParameter parameter ? parameter : AppInstallerCommandParameter.Empty), param => this.CanExecuteAppInstaller(param is AppInstallerCommandParameter parameter ? parameter : AppInstallerCommandParameter.Empty));

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

        public ICommand UpdateImpact { get; }

        public ICommand Winget { get; }
        
        public ICommand OpenResign { get; }

        public ICommand CertManager { get; }

        public ICommand MountRegistry { get; }

        public ICommand OpenExplorer { get; }

        public ICommand Deprovision { get; }

        public ICommand OpenExplorerUser { get; }

        public ICommand ChangeVolume { get; }

        public ICommand OpenManifest { get; }

        public ICommand OpenConfigJson { get; }

        public ICommand RunApp { get; }

        public ICommand StopApp { get; }

        public ICommand RunTool { get; }

        public ICommand AddPackage { get; }

        public ICommand OpenAppsFeatures { get; }

        public ICommand OpenDevSettings { get; }

        public ICommand Copy { get; }

        private async void RefreshExecute()
        {
            var executor = this.application.CommandExecutor
                .WithErrorHandling(this.interactionService, true)
                .WithBusyManager(this.busyManager, OperationType.PackageLoading);

            await executor.Invoke(this, new GetPackagesCommand(), CancellationToken.None).ConfigureAwait(false);
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

            AddPackageOptions options = 0;
            if (forAllUsers)
            {
                options |= AddPackageOptions.AllUsers;
            }

            options |= AddPackageOptions.AllowDowngrade;
            options |= AddPackageOptions.KillRunningApps;

            var context = this.busyManager.Begin();
            try
            {
                using (var wrappedProgress = new WrappedProgress(context))
                {
                    var p1 = wrappedProgress.GetChildProgress(90);
                    var p2 = wrappedProgress.GetChildProgress(10);

                    var manager = await this.packageManagerProvider.GetProxyFor(forAllUsers ? SelfElevationLevel.AsAdministrator : SelfElevationLevel.AsInvoker).ConfigureAwait(false);
                    await manager.Add(selection, options, progress: p1).ConfigureAwait(false);
                   
                    AppxManifestSummary appxReader;

                    var allPackages = await this.application.CommandExecutor.Invoke<GetPackagesCommand, IList<InstalledPackage>> (this, new GetPackagesCommand(forAllUsers ? PackageFindMode.AllUsers : PackageFindMode.CurrentUser), progress: p2).ConfigureAwait(false);
                    
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
                        if (selected != null)
                        {
                            await this.application.CommandExecutor.Invoke(this, new SelectPackagesCommand(selected.ManifestLocation)).ConfigureAwait(false);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                this.interactionService.ShowError("The package could not be added.", exception);
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

        private async void MountRegistryExecute()
        {
            var selection = this.application.ApplicationState.Packages.SelectedPackages;
            if (selection.Count != 1)
            {
                return;
            }

            try
            {
                var manager = await this.registryManagerProvider.GetProxyFor(SelfElevationLevel.AsAdministrator).ConfigureAwait(false);
                await manager.MountRegistry(selection.First(), true).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                this.interactionService.ShowError("Could not mount the registry.", exception);
            }
        }
        private async void DeprovisionExecute()
        {
            var selection = this.application.ApplicationState.Packages.SelectedPackages;
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
                using (var pw = new WrappedProgress(context))
                {
                    var p1 = pw.GetChildProgress(80);
                    var p2 = pw.GetChildProgress(20);

                    var appManager = await this.packageManagerProvider.GetProxyFor(SelfElevationLevel.AsAdministrator).ConfigureAwait(false);
                    await appManager.Deprovision(selection.First().PackageFamilyName, CancellationToken.None, p1).ConfigureAwait(false);
                    await this.application.CommandExecutor.Invoke(this, new GetPackagesCommand(), CancellationToken.None, p2).ConfigureAwait(false);
                }
            }
            catch (Exception exception)
            {
                this.interactionService.ShowError("Could not deprovision the package.", exception);
            }
            finally
            {
                this.busyManager.End(context);
            }
        }

        private async void DismountRegistryExecute()
        {
            var selection = this.application.ApplicationState.Packages.SelectedPackages;
            if (selection.Count != 1)
            {
                return;
            }

            if (selection.First().InstallLocation == null)
            {
                return;
            }

            try
            {
                var manager = await this.registryManagerProvider.GetProxyFor(SelfElevationLevel.AsAdministrator).ConfigureAwait(false);
                await manager.DismountRegistry(selection.First()).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                this.interactionService.ShowError("Could not dismount the registry.", exception);
            }
        }
        
        private bool CanExecuteMountRegistry()
        {
            var selection = this.application.ApplicationState.Packages.SelectedPackages;
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
                var manager = this.registryManagerProvider.GetProxyFor().GetAwaiter().GetResult();
                var regState = manager.GetRegistryMountState(selected.InstallLocation, selected.Name).GetAwaiter().GetResult();
                return regState == RegistryMountState.NotMounted;
            }
            catch (Exception)
            {
                return false;
            }
        }
        
        private bool CanExecuteStopApp()
        {
            var selection = this.application.ApplicationState.Packages.SelectedPackages;
            if (selection.Count != 1)
            {
                return false;
            }

            var selected = selection.First();
            if (selected?.InstallLocation == null)
            {
                return false;
            }

            return this.application.ApplicationState.Packages.ActivePackageNames.Contains(selected.PackageId);
        }

        private bool CanExecuteDismountRegistry()
        {
            var selection = this.application.ApplicationState.Packages.SelectedPackages;
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
                var manager = this.registryManagerProvider.GetProxyFor().GetAwaiter().GetResult();
                var regState = manager.GetRegistryMountState(selected.InstallLocation, selected.Name).GetAwaiter().GetResult();
                return regState == RegistryMountState.Mounted;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void ChangeVolumeExecute()
        {
            var selection = this.application.ApplicationState.Packages.SelectedPackages;
            if (selection.Count != 1)
            {
                return;
            }

            var selected = selection.First();
            if (selected.InstallLocation == null)
            {
                return;
            }

            this.dialogService.ShowDialog(Constants.PathChangeVolume, new DialogParameters
                {
                    {
                        "path", selected.InstallLocation
                    }
                },
                result =>
                {
                });
        }

        private bool CanExecuteChangeVolume()
        {
            var selection = this.application.ApplicationState.Packages.SelectedPackages;
            if (selection.Count != 1)
            {
                return false;
            }

            var selected = selection.First();
            if (selected.InstallLocation == null)
            {
                return false;
            }

            return true;
        }

        private bool CanExecuteOpenPowerShell()
        {
            return true;
        }
        
        private bool CanExecuteSingleSelection()
        {
            var package = this.application.ApplicationState.Packages.SelectedPackages.FirstOrDefault();
            return package != null;
        }
        
        private bool CanExecuteSingleSelectionOnManifest()
        {
            var package = this.application.ApplicationState.Packages.SelectedPackages.FirstOrDefault();
            return package?.InstallLocation != null;
        }
        
        private bool CanExecuteDeprovision()
        {
            var package = this.application.ApplicationState.Packages.SelectedPackages.FirstOrDefault();
            return package != null && package.IsProvisioned;
        }

        private void OpenExplorerExecute()
        {
            var package = this.application.ApplicationState.Packages.SelectedPackages.FirstOrDefault();
            if (package == null)
            {
                return;
            }

            Process.Start("explorer.exe", "/e," + package.InstallLocation);
        }
        
        private void OpenExplorerUserExecute()
        {
            var package = this.application.ApplicationState.Packages.SelectedPackages.FirstOrDefault();
            if (package == null)
            {
                return;
            }

            Process.Start("explorer.exe", "/e," + package.UserDataPath);
        }

        private bool CanExecuteOpenExplorerUser()
        {
            var package = this.application.ApplicationState.Packages.SelectedPackages.FirstOrDefault();
            return package?.InstallLocation != null;
        }

        private void OpenManifestExecute()
        {
            var package = this.application.ApplicationState.Packages.SelectedPackages.FirstOrDefault();
            if (package == null)
            {
                return;
            }

            var config = this.configurationService.GetCurrentConfiguration().Editing ?? new EditingConfiguration();
            this.fileInvoker.Execute(config.ManifestEditorType, config.ManifestEditor, package.ManifestLocation);
        }

        private void OpenConfigJsonExecute()
        {
            var package = this.application.ApplicationState.Packages.SelectedPackages.FirstOrDefault();
            if (package == null)
            {
                return;
            }

            var config = this.configurationService.GetCurrentConfiguration().Editing ?? new EditingConfiguration();
            this.fileInvoker.Execute(config.PsfEditorType, config.PsfEditor, package.PsfConfig);
        }

        private bool CanExecuteOpenManifest()
        {
            var package = this.application.ApplicationState.Packages.SelectedPackages.FirstOrDefault();
            return package?.InstallLocation != null;
        }

        private bool CanExecuteOpenConfigJson()
        {
            var package = this.application.ApplicationState.Packages.SelectedPackages.FirstOrDefault();
            return package?.InstallLocation != null && File.Exists(package.PsfConfig);
        }

        private async void StopAppExecute()
        {
            var package = this.application.ApplicationState.Packages.SelectedPackages.FirstOrDefault();
            if (package == null)
            {
                return;
            }

            if (package.SignatureKind == SignatureKind.System)
            {
                var buttons = new string[]
                {
                    "Stop this system app",
                    "Leave the app running"
                };

                if (this.interactionService.ShowMessage("This is a system app. Are you sure you want to stop it?\r\nStopping a system app may have unexpected side-effects.",
                    buttons, "Stopping a system app", systemButtons: InteractionResult.None) != 0)
                {
                    return;
                }
            }

            var executor = this.application.CommandExecutor
                .WithBusyManager(this.busyManager, OperationType.Other)
                .WithErrorHandling(this.interactionService, true);

            await executor.Invoke(this, new StopPackageCommand(package), CancellationToken.None).ConfigureAwait(false);
        }

        private async void RunAppExecute()
        {
            var package = this.application.ApplicationState.Packages.SelectedPackages.FirstOrDefault();
            if (package == null)
            {
                return;
            }

            try
            {
                var manager = await this.packageManagerProvider.GetProxyFor().ConfigureAwait(false);
                await manager.Run(package.ManifestLocation).ConfigureAwait(false);
            }
            catch (InvalidOperationException exception)
            {
                this.interactionService.ShowError("Could not start the app. " + exception.Message, exception);
            }

            catch (Exception exception)
            {
                this.interactionService.ShowError("Could not start the app.", exception);
            }
        }

        private async void RemovePackageExecute(bool allUsersRemoval)
        {
            var selection = this.application.ApplicationState.Packages.SelectedPackages;
            if (!selection.Any())
            {
                return;
            }

            var config = await this.configurationService.GetCurrentConfigurationAsync().ConfigureAwait(false);
            if (config.UiConfiguration.ConfirmDeletion)
            {
                var options = new List<string>
                {
                    selection.Count == 1 ? "Remove selected package" : $"Remove selected {selection.Count} packages",
                    "Do not remove"
                };

                var caption = new StringBuilder();
                caption.Append("Are you sure you want to remove ");

                if (selection.Count == 1)
                {
                    caption.Append("'");
                    caption.Append(selection.First().DisplayName);
                    caption.Append("'");
                }
                else
                {
                    caption.AppendFormat("{0} apps", selection.Count);
                }

                if (allUsersRemoval)
                {
                    caption.Append(" from your computer for all user accounts? ");
                }
                else
                {
                    caption.Append(" from your user account? ");
                }

                caption.Append(" This operation is irreversible.");

                if (this.interactionService.ShowMessage(caption.ToString(), options) != 0)
                {
                    return;
                }
            }

            var context = this.busyManager.Begin();
            try
            {
                using (var wrappedProgress = new WrappedProgress(context))
                {
                    var p1 = wrappedProgress.GetChildProgress(70);
                    var p2 = wrappedProgress.GetChildProgress(30);

                    var manager = await this.packageManagerProvider.GetProxyFor(allUsersRemoval ? SelfElevationLevel.AsAdministrator : SelfElevationLevel.AsInvoker).ConfigureAwait(false);
                    await manager.Remove(selection, allUsersRemoval, progress: p1).ConfigureAwait(false);

                    await this.application.CommandExecutor.Invoke(this, new GetPackagesCommand(), progress: p2).ConfigureAwait(false);
                }
            }
            catch (Exception exception)
            {
                this.interactionService.ShowError("Could not remove the package.", exception);
            }
            finally
            {
                this.busyManager.End(context);
            }
        }

        private async void RunToolExecute(ToolListConfiguration tool)
        {
            var package = this.application.ApplicationState.Packages.SelectedPackages.FirstOrDefault();
            if (package == null || tool == null)
            {
                return;
            }

            var context = this.busyManager.Begin();
            try
            {
                var manager = await this.packageManagerProvider.GetProxyFor(tool.AsAdmin ? SelfElevationLevel.AsAdministrator : SelfElevationLevel.AsInvoker).ConfigureAwait(false);
                await manager.RunToolInContext(package, tool.Path, tool.Arguments, CancellationToken.None, context).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                this.interactionService.ShowError("The tool could not be started.", exception);
            }
            finally
            {
                this.busyManager.End(context);
            }
        }

        private bool CanRunTool(ToolListConfiguration tool)
        {
            if (this.application.ApplicationState.Packages.SelectedPackages.Count != 1)
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

            var context = this.busyManager.Begin();
            context.Message = "Installing certificate...";

            try
            {
                var signing = await this.signingManagerProvider.GetProxyFor(SelfElevationLevel.HighestAvailable).ConfigureAwait(false);
                await signing.InstallCertificate(selectedFile, CancellationToken.None, context).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                this.interactionService.ShowError("Could not install the selected certificate.", exception);
            }
            finally
            {
                this.busyManager.End(context);
            }
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

        private void UpdateImpactExecute()
        {
            this.dialogService.ShowDialog(Constants.PathUpdateImpact, new DialogParameters(), this.OnDialogClosed);
        }

        private void PackExecute()
        {
            this.dialogService.ShowDialog(Constants.PathPack, new DialogParameters(), this.OnDialogClosed);
        }

        private bool CanExecuteSingleSelection(bool forSelection)
        {
            return !forSelection || this.application.ApplicationState.Packages.SelectedPackages.Count == 1;
        }

        private bool CanExecuteAppInstaller(AppInstallerCommandParameter param)
        {
            switch (param)
            {
                case AppInstallerCommandParameter.Empty:
                case AppInstallerCommandParameter.Browse:
                    return true;
                case AppInstallerCommandParameter.Selection:
                    return this.application.ApplicationState.Packages.SelectedPackages.Count == 1;
                default:
                    return false;
            }
        }
        private bool CanExecutePackageExpert()
        {
            return this.application.ApplicationState.Packages.SelectedPackages.Count == 1;
        }

        private void PackageExpertExecute()
        {
            if (this.application.ApplicationState.Packages.SelectedPackages.Count != 1)
            {
                return;
            }

            var parameters = new PackageExpertSelection(this.application.ApplicationState.Packages.SelectedPackages.First().ManifestLocation).ToDialogParameters();
            this.dialogService.ShowDialog(Constants.PathPackageExpert, parameters, this.OnDialogClosed);
        }

        private void WingetExecute(AppInstallerCommandParameter parameter)
        {
            switch (parameter)
            {
                case AppInstallerCommandParameter.Empty:
                    this.dialogService.ShowDialog(Constants.PathWinget, new DialogParameters(), this.OnDialogClosed);
                    break;
                case AppInstallerCommandParameter.Selection:
                    if (this.application.ApplicationState.Packages.SelectedPackages.Count != 1)
                    {
                        this.dialogService.ShowDialog(Constants.PathWinget, new DialogParameters(),
                            this.OnDialogClosed);
                    }
                    else
                    {
                        var parameters = new DialogParameters
                        {
                            { "msix", this.application.ApplicationState.Packages.SelectedPackages.First().ManifestLocation}
                        };

                        this.dialogService.ShowDialog(Constants.PathWinget, parameters, this.OnDialogClosed);
                    }

                    break;
                case AppInstallerCommandParameter.Browse:
                    if (this.interactionService.SelectFile("Winget manifets (*.yaml)|*.yaml|All files|*.*",
                        out var selected))
                    {
                        var parameters = new DialogParameters
                        {
                            { "yaml" , selected}
                        };

                        this.dialogService.ShowDialog(Constants.PathWinget, parameters, this.OnDialogClosed);
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(parameter), parameter, null);
            }
        }

        private void AppInstallerExecute(AppInstallerCommandParameter parameter)
        {
            switch (parameter)
            {
                case AppInstallerCommandParameter.Empty:
                    this.dialogService.ShowDialog(Constants.PathAppInstaller, new DialogParameters(), this.OnDialogClosed);
                    break;
                case AppInstallerCommandParameter.Selection:
                    if (this.application.ApplicationState.Packages.SelectedPackages.Count != 1)
                    {
                        this.dialogService.ShowDialog(Constants.PathAppInstaller, new DialogParameters(), this.OnDialogClosed);
                    }
                    else
                    {
                        var parameters = new DialogParameters
                        {
                            { "file", this.application.ApplicationState.Packages.SelectedPackages.First().ManifestLocation }
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
            if (!forSelection || this.application.ApplicationState.Packages.SelectedPackages.Count != 1)
            {
                this.dialogService.ShowDialog(Constants.PathModificationPackage, new DialogParameters(), this.OnDialogClosed);
            }
            else
            {
                var parameters = new DialogParameters
                {
                    { "file", this.application.ApplicationState.Packages.SelectedPackages.First().ManifestLocation }
                };

                this.dialogService.ShowDialog(Constants.PathModificationPackage, parameters, this.OnDialogClosed);
            }
        }

        private void AppAttachExecute(bool forSelection)
        {
            if (!forSelection || this.application.ApplicationState.Packages.SelectedPackages.Count != 1)
            {
                this.dialogService.ShowDialog(Constants.PathAppAttach, new DialogParameters(), this.OnDialogClosed);
            }
            else
            {
                var parameters = new DialogParameters
                {
                    { "file", this.application.ApplicationState.Packages.SelectedPackages.First().ManifestLocation }
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
            if (!this.application.ApplicationState.Packages.SelectedPackages.Any())
            {
                return;
            }

            var sb = new StringBuilder();

            foreach (var item in this.application.ApplicationState.Packages.SelectedPackages)
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
