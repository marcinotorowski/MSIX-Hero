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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Otor.MsixHero.App.Helpers;
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Hero.Commands.Packages;
using Otor.MsixHero.App.Hero.Executor;
using Otor.MsixHero.Appx.Diagnostic.Registry;
using Otor.MsixHero.Appx.Diagnostic.Registry.Enums;
using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Appx.Packaging.Installation;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;
using Otor.MsixHero.Appx.Packaging.Installation.Enums;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities.Summary;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Helpers;
using Otor.MsixHero.Infrastructure.Logging;
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
        private static readonly ILog Logger = LogManager.GetLogger(typeof(PackagesManagementCommandHandler));
        
        private readonly IMsixHeroApplication application;
        private readonly IInteractionService interactionService;
        private readonly IConfigurationService configurationService;
        private readonly IDialogService dialogService;
        private readonly IModuleManager moduleManager;
        private readonly ISelfElevationProxyProvider<IAppxPackageManager> packageManagerProvider;
        private readonly ISelfElevationProxyProvider<IRegistryManager> registryManagerProvider;
        private readonly IBusyManager busyManager;
        private readonly FileInvoker fileInvoker;

        public PackagesManagementCommandHandler(
            IMsixHeroApplication application, 
            IInteractionService interactionService,
            IConfigurationService configurationService,
            PrismServices prismServices,
            ISelfElevationProxyProvider<IAppxPackageManager> packageManagerProvider,
            ISelfElevationProxyProvider<IRegistryManager> registryManagerProvider,
            IBusyManager busyManager)
        {
            this.application = application;
            this.interactionService = interactionService;
            this.configurationService = configurationService;
            this.dialogService = prismServices.DialogService;
            this.moduleManager = prismServices.ModuleManager;
            this.packageManagerProvider = packageManagerProvider;
            this.registryManagerProvider = registryManagerProvider;
            this.busyManager = busyManager;
            this.fileInvoker = new FileInvoker(this.interactionService, this.configurationService);

            this.Refresh = new DelegateCommand(this.OnRefresh, this.CanRefresh);
            this.AddPackage = new DelegateCommand<object>(this.OnAddPackage, this.CanAddPackage);
            this.OpenExplorer = new DelegateCommand(this.OnOpenExplorer, this.CanOpenExplorer);
            this.OpenUserExplorer = new DelegateCommand(this.OnOpenUserExplorer, this.CanOpenUserExplorer);
            this.OpenManifest = new DelegateCommand(this.OnOpenManifest, this.CanOpenManifest);
            this.OpenConfigJson = new DelegateCommand(this.OnOpenConfigJson, this.CanOpenPsfConfig);
            this.OpenStore = new DelegateCommand(this.OnOpenStore, this.CanOpenStore);
            this.CheckUpdates = new DelegateCommand(this.OnCheckUpdates, this.CanCheckUpdates);
            this.RunTool = new DelegateCommand<object>(this.OnRunTool, this.CanRunTool);
            this.RemovePackage = new DelegateCommand(this.OnRemovePackage, this.CanRemovePackage);
            this.Copy = new DelegateCommand<object>(this.OnCopy, this.CanCopy);
            this.ViewDependencies = new DelegateCommand(this.OnViewDependencies, this.CanViewDependencies);
            this.ChangeVolume = new DelegateCommand(this.OnChangeVolume, this.CanChangeVolume);
            this.ShowAppInstallerDialog = new DelegateCommand<object>(this.OnShowAppInstallerDialog);
            this.ShowModificationPackageDialog = new DelegateCommand<object>(this.OnShowModificationPackageDialog);
            this.ShowWingetDialog = new DelegateCommand<object>(this.OnShowWingetDialog);
            this.MountRegistry = new DelegateCommand(this.OnMountRegistry, this.CanMountRegistry);
            this.DismountRegistry = new DelegateCommand(this.OnDismountRegistry, this.CanDismountRegistry);
            this.StartApp = new DelegateCommand<object>(this.OnStartApp, this.CanStartApp);
            this.StopApp = new DelegateCommand(this.OnStopApp, this.CanStopApp);
            this.Settings = new DelegateCommand(this.OnSettings);
        }

        public ICommand ShowAppInstallerDialog { get; }

        public ICommand ShowModificationPackageDialog { get; }

        public ICommand ShowWingetDialog { get; }

        public ICommand Refresh { get; }

        public ICommand Settings { get; }

        public ICommand ViewDependencies { get; }

        public ICommand ChangeVolume { get; }

        public ICommand AddPackage { get; }

        public ICommand OpenExplorer { get; }

        public ICommand OpenUserExplorer { get; }

        public ICommand OpenManifest { get; }

        public ICommand OpenConfigJson { get; }

        public ICommand OpenStore { get; }

        public ICommand CheckUpdates { get; }

        public ICommand RunTool { get; }
        
        public ICommand RemovePackage { get; }

        public ICommand Copy { get; }

        public ICommand MountRegistry { get; }

        public ICommand DismountRegistry { get; }

        public ICommand StartApp { get; }

        public ICommand StopApp { get; }

        private void OnSettings()
        {
            this.moduleManager.LoadModule(ModuleNames.Dialogs.Settings);
            var parameters = new DialogParameters {{"tab", "tools"}};
            this.dialogService.ShowDialog(NavigationPaths.DialogPaths.Settings, parameters, this.OnDialogOpened);
        }

        private async void OnStopApp()
        {
            var package = this.application.ApplicationState.Packages.SelectedPackages.FirstOrDefault();
            if (package == null)
            {
                return;
            }

            if (package.SignatureKind == SignatureKind.System)
            {
                var buttons = new[]
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

        //private async void OnStartApp()
        //{
        //    var package = this.application.ApplicationState.Packages.SelectedPackages.FirstOrDefault();
        //    if (package == null)
        //    {
        //        return;
        //    }

        //    try
        //    {
        //        var manager = await this.packageManagerProvider.GetProxyFor().ConfigureAwait(false);
        //        await manager.Run(package.ManifestLocation).ConfigureAwait(false);
        //    }
        //    catch (InvalidOperationException exception)
        //    {
        //        this.interactionService.ShowError("Could not start the app. " + exception.Message, exception);
        //    }

        //    catch (Exception exception)
        //    {
        //        this.interactionService.ShowError("Could not start the app.", exception);
        //    }
        //}

        //private bool CanStartApp()
        //{
        //    var package = this.application.ApplicationState.Packages.SelectedPackages.FirstOrDefault();
        //    return package?.InstallLocation != null;
        //}

        private bool CanStopApp()
        {
            if (this.application.ApplicationState.Packages.ActivePackageNames == null)
            {
                return false;
            }

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

            return this.application.ApplicationState.Packages.ActivePackageNames?.Contains(selected.PackageId) == true;
        }

        private void OnShowWingetDialog(object commandParameter)
        {
            if (!(commandParameter is DialogTarget dialogTarget))
            {
                dialogTarget = DialogTarget.Selection;
            }

            switch (dialogTarget)
            {
                case DialogTarget.Empty:
                    this.OpenEmptyDialog(
                        ModuleNames.Dialogs.Winget,
                        NavigationPaths.DialogPaths.WingetYamlEditor);
                    break;
                case DialogTarget.Ask:
                    this.OpenBrowseDialog(
                        ModuleNames.Dialogs.Winget,
                        NavigationPaths.DialogPaths.WingetYamlEditor,
                        "yaml",
                        "Winget manifests (*.yaml)|*.yaml|All files|*.*");
                    break;
                case DialogTarget.Selection:
                    this.OpenSelectionDialog(
                        ModuleNames.Dialogs.Winget,
                        NavigationPaths.DialogPaths.WingetYamlEditor,
                        "msix");
                    break;
            }
        }

        private void OnShowModificationPackageDialog(object commandParameter)
        {
            if (!(commandParameter is DialogTarget dialogTarget))
            {
                dialogTarget = DialogTarget.Selection;
            }

            switch (dialogTarget)
            {
                case DialogTarget.Empty:
                    this.OpenEmptyDialog(
                        ModuleNames.Dialogs.Packaging,
                        NavigationPaths.DialogPaths.PackagingModificationPackage);
                    break;
                case DialogTarget.Ask:
                    this.OpenBrowseDialog(
                        ModuleNames.Dialogs.Packaging,
                        NavigationPaths.DialogPaths.PackagingModificationPackage,
                        "file",
                        $"MSIX packages (*{FileConstants.MsixExtension})|*{FileConstants.MsixExtension}|All files|*.*");
                    break;
                case DialogTarget.Selection:
                    this.OpenSelectionDialog(
                        ModuleNames.Dialogs.Packaging,
                        NavigationPaths.DialogPaths.PackagingModificationPackage);
                    break;
            }
        }

        private void OnShowAppInstallerDialog(object commandParameter)
        {
            if (!(commandParameter is DialogTarget dialogTarget))
            {
                dialogTarget = DialogTarget.Selection;
            }

            switch (dialogTarget)
            {
                case DialogTarget.Empty:
                    this.OpenEmptyDialog(
                        ModuleNames.Dialogs.AppInstaller,
                        NavigationPaths.DialogPaths.AppInstallerEditor);
                    break;
                case DialogTarget.Ask:
                    this.OpenBrowseDialog(
                        ModuleNames.Dialogs.AppInstaller,
                        NavigationPaths.DialogPaths.AppInstallerEditor,
                        "file",
                        "App installer files|*.appinstaller|All files|*.*");
                    break;
                case DialogTarget.Selection:
                    this.OpenSelectionDialog(
                        ModuleNames.Dialogs.AppInstaller,
                        NavigationPaths.DialogPaths.AppInstallerEditor);
                    break;
            }
        }

        private async void OnMountRegistry()
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

        private async void OnDismountRegistry()
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

        private bool CanMountRegistry()
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
                var manager = this.registryManagerProvider.GetProxyFor(SelfElevationLevel.AsInvoker).GetAwaiter().GetResult();
                var regState = manager.GetRegistryMountState(selected.InstallLocation, selected.Name).GetAwaiter().GetResult();
                return regState == RegistryMountState.NotMounted;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool CanDismountRegistry()
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
                var manager = this.registryManagerProvider.GetProxyFor(SelfElevationLevel.AsInvoker).GetAwaiter().GetResult();
                var regState = manager.GetRegistryMountState(selected.InstallLocation, selected.Name).GetAwaiter().GetResult();
                return regState == RegistryMountState.Mounted;
            }
            catch (Exception)
            {
                return false;
            }
        }

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
                    if (!this.interactionService.SelectFile(FileDialogSettings.FromFilterString(new DialogFilterBuilder( "*" + FileConstants.MsixExtension, "*" + FileConstants.AppxExtension).BuildFilter()), out packagePath))
                    {
                        return;
                    }
                }
                else
                {
                    if (!this.interactionService.SelectFile(
                        // ReSharper disable StringLiteralTypo
                        FileDialogSettings.FromFilterString(new DialogFilterBuilder("*" + FileConstants.MsixExtension, "*" + FileConstants.AppxExtension, "*" + FileConstants.AppxBundleExtension, "*" + FileConstants.AppInstallerExtension, FileConstants.AppxManifestFile).BuildFilter()), out packagePath))
                        // ReSharper restore StringLiteralTypo
                    {
                        return;
                    }
                }
            }

            AddAppxPackageOptions options = 0;

            if (forAllUsers)
            {
                options |= AddAppxPackageOptions.AllUsers;
            }

            options |= AddAppxPackageOptions.KillRunningApps;

            var context = this.busyManager.Begin();
            try
            {
                using (var wrappedProgress = new WrappedProgress(context))
                {
                    var p1 = wrappedProgress.GetChildProgress(90);
                    var p2 = wrappedProgress.GetChildProgress(10);

                    var manager = await this.packageManagerProvider.GetProxyFor(forAllUsers ? SelfElevationLevel.AsAdministrator : SelfElevationLevel.AsInvoker).ConfigureAwait(false);
                    await manager.Add(packagePath, options, progress: p1).ConfigureAwait(false);

                    AppxIdentity appxIdentity = null;
                    if (!string.Equals(FileConstants.AppInstallerExtension, Path.GetExtension(packagePath), StringComparison.OrdinalIgnoreCase))
                    {
                        appxIdentity = await new AppxIdentityReader().GetIdentity(packagePath).ConfigureAwait(false);

#pragma warning disable 4014
                        this.interactionService.ShowToast("App installed", $"{appxIdentity.Name} has been just installed.", InteractionType.None);
#pragma warning restore 4014
                    }
                    else
                    {
#pragma warning disable 4014
                        this.interactionService.ShowToast("App installed", $"A new app has been just installed from {Path.GetFileName(packagePath)}.", InteractionType.None);
#pragma warning restore 4014
                    }

                    var allPackages = await this.application.CommandExecutor.Invoke<GetPackagesCommand, IList<InstalledPackage>>(this, new GetPackagesCommand(forAllUsers ? PackageFindMode.AllUsers : PackageFindMode.CurrentUser), progress: p2).ConfigureAwait(false);
                    
                    if (appxIdentity != null)
                    {
                        var selected = allPackages.FirstOrDefault(p => p.Name == appxIdentity.Name);
                        if (selected != null)
                        {
                            //this.application.ApplicationState.Packages.SelectedPackages.Clear();
                            //this.application.ApplicationState.Packages.SelectedPackages.Add(selected);

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
                Logger.Error(exception);
                this.interactionService.ShowError(exception.Message, exception);
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

        private async void OnStartApp(object parameter)
        {
            var selection = this.GetSingleOrDefaultSelection();
            if (selection == null)
            {
                return;
            }

            try
            {
                var manager = await this.packageManagerProvider.GetProxyFor(SelfElevationLevel.AsInvoker).ConfigureAwait(false);
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

        private bool CanStartApp(object parameter) => this.IsSingleSelected();

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

        private bool CanChangeVolume() => this.GetSingleOrDefaultSelection()?.InstallLocation != null;

        private void OnChangeVolume()
        {
            var selection = this.GetSingleOrDefaultSelection();
            if (selection?.InstallLocation == null)
            {
                return;
            }

            this.moduleManager.LoadModule(ModuleNames.Dialogs.Volumes);
            IDialogParameters parameters = new DialogParameters();
            parameters.Add("file", this.GetSingleOrDefaultSelection().InstallLocation);

            this.dialogService.ShowDialog(NavigationPaths.DialogPaths.VolumesChangeVolume, parameters, this.OnDialogOpened);
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

                    var manager = await this.packageManagerProvider.GetProxyFor(SelfElevationLevel.AsInvoker).ConfigureAwait(false);
                    var removedPackageNames = this.application.ApplicationState.Packages.SelectedPackages.Select(p => p.DisplayName).ToArray();
                    var removedPackages = this.application.ApplicationState.Packages.SelectedPackages.Select(p => p.PackageId).ToArray();
                    await manager.Remove(removedPackages, progress: p1).ConfigureAwait(false);

                    await this.application.CommandExecutor.Invoke(this, new SelectPackagesCommand()).ConfigureAwait(false);

                    switch (removedPackages.Length)
                    {
                        case 1:
#pragma warning disable 4014
                            this.interactionService.ShowToast("App removed", $"{removedPackageNames.FirstOrDefault()} has been just removed.", InteractionType.None);
#pragma warning restore 4014
                            break;
                        default:
#pragma warning disable 4014
                            this.interactionService.ShowToast("Apps removed", $"{removedPackages.Length} apps has been just removed.", InteractionType.None);
#pragma warning restore 4014
                            break;
                    }

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

        private bool CanRemovePackage() => this.IsAnySelected();

        private bool CanOpenStore() => this.IsSingleSelected() && this.application.ApplicationState.Packages.SelectedPackages.FirstOrDefault()?.SignatureKind == SignatureKind.Store;

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
            var requiredParameter = (PackageProperty)parameter;

            var toCopy = new StringBuilder();

            foreach (var pkg in this.application.ApplicationState.Packages.SelectedPackages)
            {
                switch (requiredParameter)
                {
                    case PackageProperty.Name:
                        toCopy.AppendLine(pkg.Name);
                        break;
                    case PackageProperty.DisplayName:
                        toCopy.AppendLine(pkg.DisplayName);
                        break;
                    case PackageProperty.FullName:
                        toCopy.AppendLine(pkg.PackageId);
                        break;
                    case PackageProperty.Version:
                        toCopy.AppendLine(pkg.Version.ToString());
                        break;
                    case PackageProperty.Publisher:
                        toCopy.AppendLine(pkg.DisplayPublisherName);
                        break;
                    case PackageProperty.Subject:
                        toCopy.AppendLine(pkg.Publisher);
                        break;
                    case PackageProperty.InstallPath:
                        toCopy.AppendLine(pkg.InstallLocation);
                        break;
                }
            }

            Clipboard.SetText(toCopy.ToString().TrimEnd(), TextDataFormat.Text);
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
        
        private void OpenBrowseDialog(
            string moduleName,
            string navigationPath,
            string fileParameterName = "file",
            string fileFilter = null)
        {
            if (!this.interactionService.SelectFile(FileDialogSettings.FromFilterString(fileFilter), out var selected))
            {
                return;
            }

            var parameters = new DialogParameters
            {
                { fileParameterName, selected }
            };

            this.moduleManager.LoadModule(moduleName);
            this.dialogService.ShowDialog(navigationPath, parameters, this.OnDialogClosed);
        }

        private void OpenEmptyDialog(
            string moduleName,
            string navigationPath)
        {
            var parameters = new DialogParameters();
            this.moduleManager.LoadModule(moduleName);
            this.dialogService.ShowDialog(navigationPath, parameters, this.OnDialogClosed);
        }

        private void OpenSelectionDialog(
            string moduleName,
            string navigationPath,
            string fileParameterName = "file",
            Func<InstalledPackage, string> valueGetter = null)
        {
            var selected = this.application.ApplicationState.Packages.SelectedPackages.FirstOrDefault();
            if (selected == null)
            {
                return;
            }
                
            var parameters = new DialogParameters
            {
                { fileParameterName, valueGetter == null ? selected.ManifestLocation : valueGetter(selected) }
            };

            this.moduleManager.LoadModule(moduleName);
            this.dialogService.ShowDialog(navigationPath, parameters, this.OnDialogClosed);
        }

        private void OnDialogClosed(IDialogResult obj)
        {
        }
    }
}
