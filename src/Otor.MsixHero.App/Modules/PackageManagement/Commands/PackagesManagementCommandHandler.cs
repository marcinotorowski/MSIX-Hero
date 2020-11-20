using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Hero.Commands.Packages;
using Otor.MsixHero.App.Hero.Executor;
using Otor.MsixHero.Appx.Packaging.Installation;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;
using Otor.MsixHero.Appx.Packaging.Installation.Enums;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities.Summary;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Helpers;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation.Enums;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Lib.Infrastructure.Progress;
using Prism.Commands;
using Prism.Modularity;
using Prism.Services.Dialogs;

namespace Otor.MsixHero.App.Modules.PackageManagement.Commands
{
    public class PackagesManagementCommandHandler
    {
        private readonly IMsixHeroApplication application;
        private readonly IInteractionService interactionService;
        private readonly IConfigurationService configurationService;
        private readonly IDialogService dialogService;
        private readonly IModuleManager moduleManager;
        private readonly ISelfElevationProxyProvider<IAppxPackageManager> packageManagerProvider;
        private readonly IBusyManager busyManager;
        private readonly FileInvoker fileInvoker;

        public PackagesManagementCommandHandler(
            IMsixHeroApplication application, 
            IInteractionService interactionService,
            IConfigurationService configurationService,
            PrismServices prismServices,
            ISelfElevationProxyProvider<IAppxPackageManager> packageManagerProvider,
            IBusyManager busyManager)
        {
            this.application = application;
            this.interactionService = interactionService;
            this.configurationService = configurationService;
            this.dialogService = prismServices.DialogService;
            this.moduleManager = prismServices.ModuleManager;
            this.packageManagerProvider = packageManagerProvider;
            this.busyManager = busyManager;
            this.fileInvoker = new FileInvoker(this.interactionService);

            this.Refresh = new DelegateCommand(this.OnRefresh, this.CanRefresh);
            this.AddPackage = new DelegateCommand<object>(this.OnAddPackage, this.CanAddPackage);
            this.OpenExplorer = new DelegateCommand(this.OnOpenExplorer, this.CanOpenExplorer);
            this.OpenUserExplorer = new DelegateCommand(this.OnOpenUserExplorer, this.CanOpenUserExplorer);
            this.OpenManifest = new DelegateCommand(this.OnOpenManifest, this.CanOpenManifest);
            this.OpenConfigJson = new DelegateCommand(this.OnOpenConfigJson, this.CanOpenPsfConfig);
            this.OpenStore = new DelegateCommand(this.OnOpenStore, this.CanOpenStore);
            this.CheckUpdates = new DelegateCommand(this.OnCheckUpdates, this.CanCheckUpdates);
            this.RunTool = new DelegateCommand<object>(this.OnRunTool, this.CanRunTool);
            this.RunPackage = new DelegateCommand<object>(this.OnRunPackage, this.CanRunPackage);
            this.RemovePackage = new DelegateCommand(this.OnRemovePackage, this.CanRemovePackage);
            this.Copy = new DelegateCommand<object>(this.OnCopy, this.CanCopy);
            this.ViewDependencies = new DelegateCommand(this.OnViewDependencies, this.CanViewDependencies);
        }

        public ICommand Refresh { get; }

        public ICommand ViewDependencies { get; }

        public ICommand AddPackage { get; }

        public ICommand OpenExplorer { get; }

        public ICommand OpenUserExplorer { get; }

        public ICommand OpenManifest { get; }

        public ICommand OpenConfigJson { get; }

        public ICommand OpenStore { get; }

        public ICommand CheckUpdates { get; }

        public ICommand RunTool { get; }

        public ICommand RunPackage { get; }

        public ICommand RemovePackage { get; }

        public ICommand Copy { get; }

        private async void OnRefresh()
        {
            var executor = this.application.CommandExecutor
                .WithErrorHandling(this.interactionService, true)
                .WithBusyManager(this.busyManager, OperationType.PackageLoading);

            await executor.Invoke(this, new GetPackagesCommand(), CancellationToken.None).ConfigureAwait(false);
        }

        private async void OnAddPackage(string packagePath, bool forAllUsers)
        {
            if (packagePath == null)
            {
                if (forAllUsers)
                {
                    if (!this.interactionService.SelectFile("All supported files|*.msix;*.appx|Packages|*.msix;*.appx", out packagePath))
                    {
                        return;
                    }
                }
                else
                {
                    if (!this.interactionService.SelectFile("All supported files|*.msix;*.appx;*.appxbundle;*.appinstaller;AppxManifest.xml|Packages and bundles|*.msix;*.appx;*.appxbundle|App installer files|*.appinstaller|Manifest files|AppxManifest.xml", out packagePath))
                    {
                        return;
                    }
                }
            }

            AddPackageOptions options = 0;

            if (forAllUsers)
            {
                options |= AddPackageOptions.AllUsers;
            }

            options |= AddPackageOptions.KillRunningApps;

            var context = this.busyManager.Begin();
            try
            {
                using (var wrappedProgress = new WrappedProgress(context))
                {
                    var p1 = wrappedProgress.GetChildProgress(90);
                    var p2 = wrappedProgress.GetChildProgress(10);

                    var manager = await this.packageManagerProvider.GetProxyFor(forAllUsers ? SelfElevationLevel.AsAdministrator : SelfElevationLevel.AsInvoker).ConfigureAwait(false);
                    await manager.Add(packagePath, options, progress: p1).ConfigureAwait(false);

                    AppxManifestSummary appxReader;

                    var allPackages = await this.application.CommandExecutor.Invoke<GetPackagesCommand, IList<InstalledPackage>>(this, new GetPackagesCommand(forAllUsers ? PackageFindMode.AllUsers : PackageFindMode.CurrentUser), progress: p2).ConfigureAwait(false);

                    if (!string.Equals(".appinstaller", Path.GetExtension(packagePath), StringComparison.OrdinalIgnoreCase))
                    {
                        appxReader = await AppxManifestSummaryBuilder.FromFile(packagePath, AppxManifestSummaryBuilderMode.Identity).ConfigureAwait(false);
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
                    else
                    {
                        await this.application.CommandExecutor.Invoke(this, new SelectPackagesCommand()).ConfigureAwait(false);
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

        private void OnAddPackage(object forAllUsers)
        {
            var forAllUsersBool = forAllUsers is bool b && b;
            this.OnAddPackage(null, forAllUsersBool);
        }

        private bool CanRefresh() => true;

        private bool CanAddPackage(object forAllUsers) => true;

        private async void OnRunPackage(object parameter)
        {
            var selection = this.GetSingleOrDefaultSelection();
            if (selection == null)
            {
                return;
            }

            try
            {
                var manager = await this.packageManagerProvider.GetProxyFor().ConfigureAwait(false);
                await manager.Run(selection.ManifestLocation, (string)parameter).ConfigureAwait(false);
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

        private bool CanRunPackage(object parameter) => parameter != null;

        private bool CanRunTool(object parameter)
        {
            if (!(parameter is ToolListConfiguration))
            {
                return false;
            }

            return this.IsSingleSelected();
        }

        private async void OnRunTool(object parameter)
        {
            if (!(parameter is ToolListConfiguration tool))
            {
                return;
            }

            var selection = this.GetSingleOrDefaultSelection();
            if (selection == null)
            {
                return;
            }

            var context = this.busyManager.Begin();
            try
            {
                var details = await selection.ToAppxPackage().ConfigureAwait(false);
                var manager = await this.packageManagerProvider.GetProxyFor(tool.AsAdmin ? SelfElevationLevel.AsAdministrator : SelfElevationLevel.AsInvoker).ConfigureAwait(false);
                await manager.RunToolInContext(selection.PackageFamilyName, details.Applications[0].Id, tool.Path, tool.Arguments, CancellationToken.None, context).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                this.interactionService.ShowError(exception.Message, exception);
            }
            finally
            {
                this.busyManager.End(context);
            }
        }
        
        private async void OnCheckUpdates()
        {
            var appInstallers = this.application.ApplicationState.Packages.SelectedPackages.Where(p => p.AppInstallerUri != null).ToArray();
            var anyStore = this.application.ApplicationState.Packages.SelectedPackages.Any(p => p.SignatureKind == SignatureKind.Store);

            if (!appInstallers.Any() && !anyStore)
            {
                return;
            }

            var executor = this.application.CommandExecutor
                .WithBusyManager(this.busyManager, OperationType.Other)
                .WithErrorHandling(this.interactionService, true);

            if (anyStore)
            {
                Process.Start(new ProcessStartInfo("ms-windows-store://downloadsandupdates") { UseShellExecute = true });
            }

            if (appInstallers.Length == 1)
            {
                var updateResult = await executor.Invoke<CheckForUpdatesCommand, AppInstallerUpdateAvailabilityResult>(this, new CheckForUpdatesCommand(appInstallers[0].PackageId)).ConfigureAwait(false);
                string msg;

                var askForUpdate = false;
                switch (updateResult)
                {
                    case AppInstallerUpdateAvailabilityResult.Unknown:
                        msg = "This package was not installed via .appinstaller file.";
                        break;
                    case AppInstallerUpdateAvailabilityResult.NoUpdates:
                        msg = "No updates are available.";
                        break;
                    case AppInstallerUpdateAvailabilityResult.Available:
                        msg = "An optional update is available.";
                        askForUpdate = true;
                        break;
                    case AppInstallerUpdateAvailabilityResult.Required:
                        msg = "A required update is available.";
                        askForUpdate = true;
                        break;
                    case AppInstallerUpdateAvailabilityResult.Error:
                        msg = "Could not check for updates.";
                        break;
                    default:
                        msg = "Could not check for updates.";
                        break;
                }

                if (!askForUpdate)
                {
                    this.interactionService.ShowInfo(msg, InteractionResult.OK, "Update check result");
                }
                else
                {
                    if (this.interactionService.ShowMessage(msg, new[] { "Update now" }, "Update check result", systemButtons: InteractionResult.Close) == 0)
                    {
                        this.OnAddPackage(appInstallers[0].AppInstallerUri.ToString(), false);
                    }
                }
            }
            else if (appInstallers.Length > 1)
            {
                var updateResults = new Dictionary<AppInstallerUpdateAvailabilityResult, IList<string>>();

                foreach (var item in appInstallers)
                {
                    var updateResult = await executor.Invoke<CheckForUpdatesCommand, AppInstallerUpdateAvailabilityResult>(this, new CheckForUpdatesCommand(item.PackageId)).ConfigureAwait(false);

                    if (!updateResults.TryGetValue(updateResult, out var list))
                    {
                        list = new List<string>();
                        updateResults[updateResult] = list;
                    }

                    list.Add(item.DisplayName);
                }

                var stringBuilder = new StringBuilder();

                foreach (var key in updateResults.Keys)
                {
                    switch (key)
                    {
                        case AppInstallerUpdateAvailabilityResult.Unknown:
                            stringBuilder.AppendLine("These package were not installed via .appinstaller file:");
                            break;
                        case AppInstallerUpdateAvailabilityResult.NoUpdates:
                            stringBuilder.AppendLine("No updates are available for these packages:");
                            break;
                        case AppInstallerUpdateAvailabilityResult.Available:
                            stringBuilder.AppendLine("An optional update is available for these packages:");
                            break;
                        case AppInstallerUpdateAvailabilityResult.Required:
                            stringBuilder.AppendLine("A required update is available for these packages:");
                            break;
                        case AppInstallerUpdateAvailabilityResult.Error:
                            stringBuilder.AppendLine("Could not check for updates for these packages:");
                            break;
                        default:
                            stringBuilder.AppendLine("Could not check for updates for these packages:");
                            break;
                    }

                    foreach (var item in updateResults[key])
                    {
                        stringBuilder.AppendLine(item);
                    }

                    stringBuilder.AppendLine();
                }

                if (updateResults.Keys.Any(k =>
                    k == AppInstallerUpdateAvailabilityResult.Required ||
                    k == AppInstallerUpdateAvailabilityResult.Available))
                {
                    var msg = "Updates are available for the following packages:\r\n" + string.Join("\r\n",
                        updateResults
                            .Where(k => k.Key == AppInstallerUpdateAvailabilityResult.Available ||
                                        k.Key == AppInstallerUpdateAvailabilityResult.Required).SelectMany(kv => kv.Value));

                    this.interactionService.ShowInfo(msg, InteractionResult.OK, "Update check result");
                }
                else
                {
                    this.interactionService.ShowInfo("There are no updates available.", InteractionResult.OK, "Update check result");
                }
            }
        }

        private bool CanCheckUpdates()
        {
            return this.application.ApplicationState.Packages.SelectedPackages.Any(p => p.SignatureKind == SignatureKind.Store || p.AppInstallerUri != null);
        }

        private void OnViewDependencies()
        {
            var selection = this.GetSingleOrDefaultSelection();
            if (selection == null)
            {
                return;
            }

            this.moduleManager.LoadModule(ModuleNames.Dialogs.Dependencies);
            var parameters = new DialogParameters();
            parameters.Add("file", selection.ManifestLocation);
            this.dialogService.ShowDialog(NavigationPaths.DialogPaths.DependenciesGraph, parameters, this.OnDialogOpened);
        }

        private void OnDialogOpened(IDialogResult obj)
        {
        }

        private bool CanViewDependencies() => this.IsSingleSelected();

        private async void OnRemovePackage()
        {
            if (!this.IsAnySelected())
            {
                return;
            }

            var config = await this.configurationService.GetCurrentConfigurationAsync().ConfigureAwait(false);
            if (config.UiConfiguration.ConfirmDeletion)
            {
                var options = new List<string>
                {
                    "Remove for current user",
                    "Do not remove"
                };

                var singleSelection = this.GetSingleOrDefaultSelection();
                if (singleSelection != null)
                {
                    var caption = "Are you sure you want to remove " + singleSelection.DisplayName + " " + singleSelection.Version + "? This operation is irreversible.";

                    var selectedOption = this.interactionService.ShowMessage(caption, options, "Removing package", systemButtons: InteractionResult.Cancel);
                    if (selectedOption != 0)
                    {
                        return;
                    }
                }
                else
                {
                    var selection = this.application.ApplicationState.Packages.SelectedPackages;
                    var caption = "Are you sure you want to remove " + selection.Count + " packages? This operation is irreversible.";

                    var selectedOption = this.interactionService.ShowMessage(caption, options, "Removing package", systemButtons: InteractionResult.Cancel);
                    if (selectedOption != 0)
                    {
                        return;
                    }
                }
            }

            var context = this.busyManager.Begin();
            try
            {
                using (var wrappedProgress = new WrappedProgress(context))
                {
                    var p1 = wrappedProgress.GetChildProgress(70);
                    var p2 = wrappedProgress.GetChildProgress(30);

                    var manager = await this.packageManagerProvider.GetProxyFor().ConfigureAwait(false);
                    await manager.Remove(this.application.ApplicationState.Packages.SelectedPackages.Select(p => p.PackageId).ToArray(), progress: p1).ConfigureAwait(false);

                    await this.application.CommandExecutor.Invoke(this, new GetPackagesCommand(), progress: p2).ConfigureAwait(false);
                    var select = this.application.ApplicationState.Packages.AllPackages.FirstOrDefault();

                    if (select == null)
                    {
                        await this.application.CommandExecutor.Invoke(this, new SelectPackagesCommand()).ConfigureAwait(false);
                    }
                    else
                    {
                        await this.application.CommandExecutor.Invoke(this, new SelectPackagesCommand(select.ManifestLocation)).ConfigureAwait(false);
                    }
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

        private bool CanRemovePackage() => this.IsAnySelected();

        private bool CanOpenStore() => this.IsSingleSelected();

        private bool CanOpenPsfConfig()
        {
            if (!this.IsSingleSelected())
            {
                return false;
            }

            return this.application.ApplicationState.Packages.SelectedPackages[0].PackageType == MsixPackageType.BridgePsf;
        }

        private bool CanOpenManifest() => this.IsSingleSelected();

        private void OnOpenManifest()
        {
            var package = this.GetSingleOrDefaultSelection();
            if (package?.ManifestLocation == null)
            {
                return;
            }

            var config = this.configurationService.GetCurrentConfiguration().Editing ?? new EditingConfiguration();
            this.fileInvoker.Execute(config.ManifestEditorType, config.ManifestEditor, package.ManifestLocation);
        }

        private void OnOpenConfigJson()
        {
            var package = this.GetSingleOrDefaultSelection();
            if (package?.PsfConfig == null)
            {
                return;
            }
            
            var config = this.configurationService.GetCurrentConfiguration().Editing ?? new EditingConfiguration();
            this.fileInvoker.Execute(config.PsfEditorType, config.PsfEditor, package.PsfConfig);
        }

        private InstalledPackage GetSingleOrDefaultSelection()
        {
            if (this.application.ApplicationState.Packages.SelectedPackages.Count != 1)
            {
                return null;
            }

            return this.application.ApplicationState.Packages.SelectedPackages.First();
        }

        private bool IsSingleSelected()
        {
            if (this.application.ApplicationState.Packages.SelectedPackages.Count != 1)
            {
                return false;
            }

            return true;
        }

        private bool IsAnySelected()
        {
            return this.application.ApplicationState.Packages.SelectedPackages.Any();
        }

        private void OnOpenStore()
        {
            var selection = this.GetSingleOrDefaultSelection();
            if (selection == null)
            {
                return;
            }

            if (selection.SignatureKind != SignatureKind.Store)
            {
                return;
            }

            var link = $"ms-windows-store://pdp/?PFN={selection.PackageFamilyName}";
            var psi = new ProcessStartInfo(link)
            {
                UseShellExecute = true
            };

            ExceptionGuard.Guard
            (
                () =>
                {
                    Process.Start(psi);
                },
                this.interactionService
            );
        }

        private void OnCopy(object parameter)
        {
            Clipboard.SetText((string)parameter);
        }

        private bool CanCopy(object parameter)
        {
            return parameter != null;
        }

        private bool CanOpenExplorer() => this.IsSingleSelected();

        private void OnOpenExplorer()
        {
            var selection = this.GetSingleOrDefaultSelection();
            if (selection?.ManifestLocation == null)
            {
                return;
            }
            
            Process.Start("explorer.exe", "/select," + selection.ManifestLocation);
        }

        private bool CanOpenUserExplorer()
        {
            var selection = this.GetSingleOrDefaultSelection();
            if (selection?.PackageFamilyName == null)
            {
                return false;
            }

            var rootDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Packages", selection.PackageFamilyName, "LocalCache");
            return Directory.Exists(rootDir);
        }

        private void OnOpenUserExplorer()
        {
            var selection = this.GetSingleOrDefaultSelection();
            if (selection?.PackageFamilyName == null)
            {
                return;
            }

            var rootDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Packages", selection.PackageFamilyName, "LocalCache");
            Process.Start("explorer.exe", "/e," + rootDir);
        }
    }
}
