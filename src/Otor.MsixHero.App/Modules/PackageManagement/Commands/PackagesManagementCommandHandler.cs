// MSIX Hero
// Copyright (C) 2022 Marcin Otorowski
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
using Otor.MsixHero.App.Mvvm.Progress;
using Otor.MsixHero.Appx.Diagnostic.Registry;
using Otor.MsixHero.Appx.Diagnostic.Registry.Enums;
using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Appx.Packaging.Installation;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;
using Otor.MsixHero.Appx.Packaging.Installation.Enums;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities.Summary;
using Otor.MsixHero.Elevation;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Helpers;
using Dapplo.Log;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Commands;
using Prism.Services.Dialogs;

namespace Otor.MsixHero.App.Modules.PackageManagement.Commands
{
    public class PackagesManagementCommandHandler
    {
        private static readonly LogSource Logger = new();        
        private readonly IMsixHeroApplication _application;
        private readonly IInteractionService _interactionService;
        private readonly IConfigurationService _configurationService;
        private readonly PrismServices _prismServices;
        private readonly IUacElevation _uacElevation;
        private readonly IBusyManager _busyManager;
        private readonly FileInvoker _fileInvoker;

        public PackagesManagementCommandHandler(
            IMsixHeroApplication application, 
            IInteractionService interactionService,
            IConfigurationService configurationService,
            PrismServices prismServices,
            IUacElevation uacElevation,
            IBusyManager busyManager)
        {
            this._application = application;
            this._interactionService = interactionService;
            this._configurationService = configurationService;
            this._prismServices = prismServices;
            this._uacElevation = uacElevation;
            this._busyManager = busyManager;
            this._fileInvoker = new FileInvoker(this._interactionService, this._configurationService);

            this.Refresh = new DelegateCommand(this.OnRefresh, this.CanRefresh);
            this.AddPackage = new DelegateCommand<object>(this.OnAddPackage, this.CanAddPackage);
            this.OpenExplorer = new DelegateCommand(this.OnOpenExplorer, this.CanOpenExplorer);
            this.OpenUserExplorer = new DelegateCommand(this.OnOpenUserExplorer, this.CanOpenUserExplorer);
            this.OpenManifest = new DelegateCommand(this.OnOpenManifest, this.CanOpenManifest);
            this.OpenConfigJson = new DelegateCommand(this.OnOpenConfigJson, this.CanOpenPsfConfig);
            this.OpenStore = new DelegateCommand(this.OnOpenStore, this.CanOpenStore);
            this.CheckUpdates = new DelegateCommand(this.OnCheckUpdates, this.CanCheckUpdates);
            this.RunTool = new DelegateCommand<object>(this.OnRunTool, this.CanRunTool);
            this.RemovePackage = new DelegateCommand<object>(o => this.OnRemovePackage(o is true), _ => this.CanRemovePackage());
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

        // ReSharper disable once IdentifierTypo
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
            this._prismServices.ModuleManager.LoadModule(ModuleNames.Dialogs.Settings);
            var parameters = new DialogParameters {{"tab", "tools"}};
            this._prismServices.DialogService.ShowDialog(NavigationPaths.DialogPaths.Settings, parameters, this.OnDialogOpened);
        }

        private async void OnStopApp()
        {
            var package = this._application.ApplicationState.Packages.SelectedPackages.FirstOrDefault();
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

                if (this._interactionService.ShowMessage("This is a system app. Are you sure you want to stop it?\r\nStopping a system app may have unexpected side-effects.",
                    buttons, "Stopping a system app", systemButtons: InteractionResult.None) != 0)
                {
                    return;
                }
            }

            var executor = this._application.CommandExecutor
                .WithBusyManager(this._busyManager, OperationType.Other)
                .WithErrorHandling(this._interactionService, true);

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
            if (this._application.ApplicationState.Packages.ActivePackageNames == null)
            {
                return false;
            }

            var selection = this._application.ApplicationState.Packages.SelectedPackages;
            if (selection.Count != 1)
            {
                return false;
            }

            var selected = selection.First();
            if (selected?.InstallLocation == null)
            {
                return false;
            }

            return this._application.ApplicationState.Packages.ActivePackageNames?.Contains(selected.PackageFamilyName) == true;
        }

        // ReSharper disable once IdentifierTypo
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

                    var filterBuilder = new DialogFilterBuilder("*" + FileConstants.WingetExtension);
                    this.OpenBrowseDialog(
                        ModuleNames.Dialogs.Winget,
                        NavigationPaths.DialogPaths.WingetYamlEditor,
                        "yaml",
                        filterBuilder.BuildFilter());
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
                    var filterBuilder = new DialogFilterBuilder("*" + FileConstants.MsixExtension);
                    this.OpenBrowseDialog(
                        ModuleNames.Dialogs.Packaging,
                        NavigationPaths.DialogPaths.PackagingModificationPackage,
                        "file",
                        filterBuilder.BuildFilter());
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

                    var filterBuilder = new DialogFilterBuilder("*" + FileConstants.AppInstallerExtension);
                    this.OpenBrowseDialog(
                        ModuleNames.Dialogs.AppInstaller,
                        NavigationPaths.DialogPaths.AppInstallerEditor,
                        "file",
                        filterBuilder.BuildFilter());
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
            var selection = this._application.ApplicationState.Packages.SelectedPackages;
            if (selection.Count != 1)
            {
                return;
            }

            try
            {
                var manager = this._uacElevation.AsAdministrator<IRegistryManager>();
                await manager.MountRegistry(selection.First(), true).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                this._interactionService.ShowError("Could not mount the registry.", exception);
            }
        }

        private async void OnDismountRegistry()
        {
            var selection = this._application.ApplicationState.Packages.SelectedPackages;
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
                var manager = this._uacElevation.AsAdministrator<IRegistryManager>();
                await manager.DismountRegistry(selection.First()).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                this._interactionService.ShowError("Could not dismount the registry.", exception);
            }
        }

        private bool CanMountRegistry()
        {
            var selection = this._application.ApplicationState.Packages.SelectedPackages;
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
                var manager = this._uacElevation.AsCurrentUser<IRegistryManager>();
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
            var selection = this._application.ApplicationState.Packages.SelectedPackages;
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
                var manager = this._uacElevation.AsCurrentUser<IRegistryManager>();
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
            var executor = this._application.CommandExecutor
                .WithErrorHandling(this._interactionService, true)
                .WithBusyManager(this._busyManager, OperationType.PackageLoading);

            await executor.Invoke<GetPackagesCommand, IList<InstalledPackage>>(this, new GetPackagesCommand(this._application.ApplicationState.Packages.Mode == PackageContext.AllUsers ? PackageFindMode.AllUsers : PackageFindMode.CurrentUser), CancellationToken.None).ConfigureAwait(false);
        }

        private async void OnAddPackage(string packagePath, bool forAllUsers)
        {
            if (packagePath == null)
            {
                if (forAllUsers)
                {
                    if (!this._interactionService.SelectFile(FileDialogSettings.FromFilterString(new DialogFilterBuilder( "*" + FileConstants.MsixExtension, "*" + FileConstants.AppxExtension).BuildFilter()), out packagePath))
                    {
                        return;
                    }
                }
                else
                {
                    if (!this._interactionService.SelectFile(
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

            var context = this._busyManager.Begin();
            try
            {
                using var wrappedProgress = new WrappedProgress(context);
                var p1 = wrappedProgress.GetChildProgress(90);
                var p2 = wrappedProgress.GetChildProgress(10);

                var manager = forAllUsers ? this._uacElevation.AsAdministrator<IAppxPackageInstaller>() : this._uacElevation.AsCurrentUser<IAppxPackageInstaller>();
                await manager.Add(packagePath, options, progress: p1).ConfigureAwait(false);

                AppxIdentity appxIdentity = null;
                if (!string.Equals(FileConstants.AppInstallerExtension, Path.GetExtension(packagePath), StringComparison.OrdinalIgnoreCase))
                {
                    appxIdentity = await new AppxIdentityReader().GetIdentity(packagePath).ConfigureAwait(false);

#pragma warning disable 4014
                    this._interactionService.ShowToast("App installed", $"{appxIdentity.Name} has been just installed.");
#pragma warning restore 4014
                }
                else
                {
#pragma warning disable 4014
                    this._interactionService.ShowToast("App installed", $"A new app has been just installed from {Path.GetFileName(packagePath)}.");
#pragma warning restore 4014
                }

                var allPackages = await this._application.CommandExecutor.Invoke<GetPackagesCommand, IList<InstalledPackage>>(this, new GetPackagesCommand(forAllUsers ? PackageFindMode.AllUsers : PackageFindMode.CurrentUser), progress: p2).ConfigureAwait(false);
                    
                if (appxIdentity != null)
                {
                    var selected = allPackages.FirstOrDefault(p => p.Name == appxIdentity.Name);
                    if (selected != null)
                    {
                        //this.application.ApplicationState.Packages.SelectedPackages.Clear();
                        //this.application.ApplicationState.Packages.SelectedPackages.Add(selected);

                        await this._application.CommandExecutor.Invoke(this, new SelectPackagesCommand(selected.PackageFullName)).ConfigureAwait(false);
                    }
                }
                else
                {
                    await this._application.CommandExecutor.Invoke(this, new SelectPackagesCommand()).ConfigureAwait(false);
                }
            }
            catch (Exception exception)
            {
                Logger.Error().WriteLine(exception);
                this._interactionService.ShowError(exception.Message, exception);
            }
            finally
            {
                this._busyManager.End(context);
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
                var manager = this._uacElevation.AsCurrentUser<IAppxPackageRunner>();
                await manager.Run(selection.ManifestLocation, (string)parameter).ConfigureAwait(false);
            }
            catch (InvalidOperationException exception)
            {
                this._interactionService.ShowError("Could not start the app. " + exception.Message, exception);
            }

            catch (Exception exception)
            {
                this._interactionService.ShowError("Could not start the app.", exception);
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

            var context = this._busyManager.Begin();
            try
            {
                var details = await selection.ToAppxPackage().ConfigureAwait(false);
                var manager = tool.AsAdmin ? this._uacElevation.AsAdministrator<IAppxPackageRunner>() : this._uacElevation.AsCurrentUser<IAppxPackageRunner>();
                await manager.RunToolInContext(selection.PackageFamilyName, details.Applications[0].Id, tool.Path, tool.Arguments, CancellationToken.None, context).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                this._interactionService.ShowError(exception.Message, exception);
            }
            finally
            {
                this._busyManager.End(context);
            }
        }
        
        private async void OnCheckUpdates()
        {
            var appInstallers = this._application.ApplicationState.Packages.SelectedPackages.Where(p => p.AppInstallerUri != null).ToArray();
            var anyStore = this._application.ApplicationState.Packages.SelectedPackages.Any(p => p.SignatureKind == SignatureKind.Store);

            if (!appInstallers.Any() && !anyStore)
            {
                return;
            }

            var executor = this._application.CommandExecutor
                .WithBusyManager(this._busyManager, OperationType.Other)
                .WithErrorHandling(this._interactionService, true);

            if (anyStore)
            {
                Process.Start(new ProcessStartInfo("ms-windows-store://downloadsandupdates") { UseShellExecute = true });
            }

            if (appInstallers.Length == 1)
            {
                var updateResult = await executor.Invoke<CheckForUpdatesCommand, AppInstallerUpdateAvailabilityResult>(this, new CheckForUpdatesCommand(appInstallers[0].PackageFullName)).ConfigureAwait(false);
                string msg;

                var askForUpdate = false;
                switch (updateResult)
                {
                    case AppInstallerUpdateAvailabilityResult.Unknown:
                        msg = "This package was not installed via " + FileConstants.AppInstallerExtension + " file.";
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
                    this._interactionService.ShowInfo(msg, InteractionResult.OK, "Update check result");
                }
                else
                {
                    if (this._interactionService.ShowMessage(msg, new[] { "Update now" }, "Update check result", systemButtons: InteractionResult.Close) == 0)
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
                    var updateResult = await executor.Invoke<CheckForUpdatesCommand, AppInstallerUpdateAvailabilityResult>(this, new CheckForUpdatesCommand(item.PackageFullName)).ConfigureAwait(false);

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
                            stringBuilder.AppendLine("These package were not installed via " + FileConstants.AppInstallerExtension + " file:");
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

                    this._interactionService.ShowInfo(msg, InteractionResult.OK, "Update check result");
                }
                else
                {
                    this._interactionService.ShowInfo("There are no updates available.", InteractionResult.OK, "Update check result");
                }
            }
        }

        private bool CanCheckUpdates()
        {
            return this._application.ApplicationState.Packages.SelectedPackages.Any(p => p.SignatureKind == SignatureKind.Store || p.AppInstallerUri != null);
        }

        private bool CanChangeVolume() => this.GetSingleOrDefaultSelection()?.InstallLocation != null;

        private void OnChangeVolume()
        {
            var selection = this.GetSingleOrDefaultSelection();
            if (selection?.InstallLocation == null)
            {
                return;
            }

            this._prismServices.ModuleManager.LoadModule(ModuleNames.Dialogs.Volumes);
            IDialogParameters parameters = new DialogParameters();
            parameters.Add("file", this.GetSingleOrDefaultSelection().InstallLocation);

            this._prismServices.DialogService.ShowDialog(NavigationPaths.DialogPaths.VolumesChangeVolume, parameters, this.OnDialogOpened);
        }

        private void OnViewDependencies()
        {
            var selection = this.GetSingleOrDefaultSelection();
            if (selection == null)
            {
                return;
            }

            this._prismServices.ModuleManager.LoadModule(ModuleNames.Dialogs.Dependencies);
            var parameters = new DialogParameters
            {
                { "file", selection.ManifestLocation }
            };

            this._prismServices.DialogService.ShowDialog(NavigationPaths.DialogPaths.DependenciesGraph, parameters, this.OnDialogOpened);
        }

        private void OnDialogOpened(IDialogResult obj)
        {
        }

        private bool CanViewDependencies() => this.IsSingleSelected();

        private async void OnRemovePackage(bool forAllUsers)
        {
            if (!this.IsAnySelected())
            {
                return;
            }

            var config = await this._configurationService.GetCurrentConfigurationAsync().ConfigureAwait(false);
            if (config.UiConfiguration.ConfirmDeletion)
            {
                var options = new List<string>
                {
                    forAllUsers ? "Remove for all users" : "Remove for current user",
                    "Do not remove"
                };

                var singleSelection = this.GetSingleOrDefaultSelection();
                if (singleSelection != null)
                {
                    var caption = "Are you sure you want to remove " + singleSelection.DisplayName + " " + singleSelection.Version + "? This operation is irreversible.";

                    var selectedOption = this._interactionService.ShowMessage(caption, options, "Removing package", systemButtons: InteractionResult.Cancel);
                    if (selectedOption != 0)
                    {
                        return;
                    }
                }
                else
                {
                    var selection = this._application.ApplicationState.Packages.SelectedPackages;
                    var caption = "Are you sure you want to remove " + selection.Count + " packages? This operation is irreversible.";

                    var selectedOption = this._interactionService.ShowMessage(caption, options, "Removing package", systemButtons: InteractionResult.Cancel);
                    if (selectedOption != 0)
                    {
                        return;
                    }
                }
            }

            var context = this._busyManager.Begin();
            try
            {
                using var wrappedProgress = new WrappedProgress(context);
                var p1 = wrappedProgress.GetChildProgress(70);
                var p2 = wrappedProgress.GetChildProgress(30);

                if (forAllUsers)
                {
                    var p0 = wrappedProgress.GetChildProgress(50);

                    var processed = 0;
                    var toProcess = this._application.ApplicationState.Packages.SelectedPackages.Count;

                    var adminManager = this._uacElevation.AsAdministrator<IAppxPackageInstaller>();

                    foreach (var item in this._application.ApplicationState.Packages.SelectedPackages)
                    {
                        p0.Report(new ProgressData((int)(100.0 * processed / toProcess), $"De-provisioning {item.Name}..."));
                        await adminManager.Deprovision(item.PackageFamilyName, CancellationToken.None).ConfigureAwait(false);
                        processed++;
                    }
                }

                var manager = this._uacElevation.AsCurrentUser<IAppxPackageInstaller>();
                var removedPackageNames = this._application.ApplicationState.Packages.SelectedPackages.Select(p => p.DisplayName).ToArray();
                var removedPackages = this._application.ApplicationState.Packages.SelectedPackages.Select(p => p.PackageFullName).ToArray();
                await manager.Remove(removedPackages, progress: p1).ConfigureAwait(false);
                
                await this._application.CommandExecutor.Invoke(this, new SelectPackagesCommand()).ConfigureAwait(false);

                switch (removedPackages.Length)
                {
                    case 1:
#pragma warning disable 4014
                        this._interactionService.ShowToast("App removed", $"{removedPackageNames.FirstOrDefault()} has been just removed.");
#pragma warning restore 4014
                        break;
                    default:
#pragma warning disable 4014
                        this._interactionService.ShowToast("Apps removed", $"{removedPackages.Length} apps has been just removed.");
#pragma warning restore 4014
                        break;
                }

                await this._application.CommandExecutor.Invoke<GetPackagesCommand, IList<InstalledPackage>>(this, new GetPackagesCommand(this._application.ApplicationState.Packages.Mode == PackageContext.AllUsers ? PackageFindMode.AllUsers : PackageFindMode.CurrentUser), progress: p2).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                this._interactionService.ShowError("Could not remove the package.", exception);
            }
            finally
            {
                this._busyManager.End(context);
            }
        }

        private bool CanRemovePackage() => this.IsAnySelected();

        private bool CanOpenStore() => this.IsSingleSelected() && this._application.ApplicationState.Packages.SelectedPackages.FirstOrDefault()?.SignatureKind == SignatureKind.Store;

        private bool CanOpenPsfConfig()
        {
            if (!this.IsSingleSelected())
            {
                return false;
            }

            return this._application.ApplicationState.Packages.SelectedPackages[0].PackageType == MsixPackageType.BridgePsf;
        }

        private bool CanOpenManifest() => this.IsSingleSelected();

        private void OnOpenManifest()
        {
            var package = this.GetSingleOrDefaultSelection();
            if (package?.ManifestLocation == null)
            {
                return;
            }

            var config = this._configurationService.GetCurrentConfiguration().Editing ?? new EditingConfiguration();
            this._fileInvoker.Execute(config.ManifestEditorType, config.ManifestEditor, package.ManifestLocation);
        }

        private void OnOpenConfigJson()
        {
            var package = this.GetSingleOrDefaultSelection();
            if (package?.PsfConfig == null)
            {
                return;
            }
            
            var config = this._configurationService.GetCurrentConfiguration().Editing ?? new EditingConfiguration();
            this._fileInvoker.Execute(config.PsfEditorType, config.PsfEditor, package.PsfConfig);
        }

        private InstalledPackage GetSingleOrDefaultSelection()
        {
            if (this._application.ApplicationState.Packages.SelectedPackages.Count != 1)
            {
                return null;
            }

            return this._application.ApplicationState.Packages.SelectedPackages.First();
        }

        private bool IsSingleSelected()
        {
            if (this._application.ApplicationState.Packages.SelectedPackages.Count != 1)
            {
                return false;
            }

            return true;
        }

        private bool IsAnySelected()
        {
            return this._application.ApplicationState.Packages.SelectedPackages.Any();
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
                this._interactionService
            );
        }

        private void OnCopy(object parameter)
        {
            if (parameter is PackageProperty requiredParameter)
            {
                var toCopy = new StringBuilder();

                foreach (var pkg in this._application.ApplicationState.Packages.SelectedPackages)
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
                            toCopy.AppendLine(pkg.PackageFullName);
                            break;
                        case PackageProperty.FamilyName:
                            toCopy.AppendLine(pkg.PackageFamilyName);
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
            else if (parameter is string stringParameter)
            {
                Clipboard.SetText(stringParameter, TextDataFormat.Text);
            }
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
            if (!this._interactionService.SelectFile(FileDialogSettings.FromFilterString(fileFilter), out var selected))
            {
                return;
            }

            var parameters = new DialogParameters
            {
                { fileParameterName, selected }
            };

            this._prismServices.ModuleManager.LoadModule(moduleName);
            this._prismServices.DialogService.ShowDialog(navigationPath, parameters, this.OnDialogClosed);
        }

        private void OpenEmptyDialog(
            string moduleName,
            string navigationPath)
        {
            var parameters = new DialogParameters();
            this._prismServices.ModuleManager.LoadModule(moduleName);
            this._prismServices.DialogService.ShowDialog(navigationPath, parameters, this.OnDialogClosed);
        }

        private void OpenSelectionDialog(
            string moduleName,
            string navigationPath,
            string fileParameterName = "file",
            Func<InstalledPackage, string> valueGetter = null)
        {
            var selected = this._application.ApplicationState.Packages.SelectedPackages.FirstOrDefault();
            if (selected == null)
            {
                return;
            }
                
            var parameters = new DialogParameters
            {
                { fileParameterName, valueGetter == null ? selected.ManifestLocation : valueGetter(selected) }
            };

            this._prismServices.ModuleManager.LoadModule(moduleName);
            this._prismServices.DialogService.ShowDialog(navigationPath, parameters, this.OnDialogClosed);
        }

        private void OnDialogClosed(IDialogResult obj)
        {
        }
    }
}
