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

using Dapplo.Log;
using Otor.MsixHero.App.Helpers.Dialogs;
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Hero.Commands.Packages;
using Otor.MsixHero.App.Hero.Executor;
using Otor.MsixHero.App.Modules.PackageManagement.Commands;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.App.Mvvm.Progress;
using Otor.MsixHero.Appx.Diagnostic.Registry;
using Otor.MsixHero.Appx.Diagnostic.Registry.Enums;
using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Appx.Packaging.Installation;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;
using Otor.MsixHero.Appx.Packaging.Installation.Enums;
using Otor.MsixHero.Appx.Packaging.Manifest;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;
using Otor.MsixHero.Appx.Packaging.Manifest.FileReaders;
using Otor.MsixHero.Elevation;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Helpers;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Otor.MsixHero.App.Modules.Dialogs.PackageExpert.ViewModels
{
    public class PackageExpertCommandHandler : NotifyPropertyChanged
    {
        private static readonly LogSource Logger = new();
        private readonly IAppxPackageQuery _query;
        private readonly IAppxPackageInstaller _packageInstaller;
        private readonly IInteractionService _interactionService;
        private readonly IConfigurationService _configurationService;
        private readonly PrismServices _prismServices;
        private readonly IUacElevation _uacElevation;
        private readonly IBusyManager _busyManager;
        private readonly FileInvoker _fileInvoker;
        private string _filePath;
        private AppxPackage _packageDetails;
        private InstalledPackage _installedPackage;
        private readonly IMsixHeroApplication _application;

        public PackageExpertCommandHandler(
            IMsixHeroApplication application,
            IAppxPackageQuery query,
            IAppxPackageInstaller packageInstaller,
            IInteractionService interactionService,
            IConfigurationService configurationService,
            PrismServices prismServices,
            IUacElevation uacElevation,
            IBusyManager busyManager)
        {
            this._application = application;
            this._query = query;
            this._packageInstaller = packageInstaller;
            this._interactionService = interactionService;
            this._configurationService = configurationService;
            this._prismServices = prismServices;
            this._uacElevation = uacElevation;
            this._busyManager = busyManager;
            this._fileInvoker = new FileInvoker(this._interactionService, this._configurationService);

            this.AddPackage = new DelegateCommand(this.InstallPackage, this.CanAddPackage);
            this.Refresh = new DelegateCommand(this.RefreshPackage, this.CanRefresh);
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
        }

        public void Reload()
        {
            if (string.IsNullOrEmpty(this.FilePath))
            {
                return;
            }

            // workaround to force refresh of bindings even if the path formally is still the same
            var first = this.FilePath[0];
            if (!char.IsUpper(first))
            {
                this.FilePath = char.ToUpperInvariant(first) + this.FilePath.Substring(1);
            }
            else
            {
                this.FilePath = char.ToLowerInvariant(first) + this.FilePath.Substring(1);
            }
        }

        public string FilePath
        {
            get => this._filePath;
            set
            {
                this._filePath = value;
                this._packageDetails = null;
                this._installedPackage = null;

                if (value != null)
                {
                    try
                    {
                        using var loader = FileReaderFactory.CreateFileReader(this._filePath);
                        var pkg = new AppxManifestReader().Read(loader).GetAwaiter().GetResult();
                        var isInstalled = this._packageInstaller.IsInstalled(this.FilePath).GetAwaiter().GetResult();
                        if (isInstalled)
                        {
                            this._installedPackage = this._query.GetInstalledPackage(pkg.FullName).GetAwaiter().GetResult();
                        }

                        this._packageDetails = pkg;

                        this.HasError = false;
                        this.ErrorMessage = null;
                    }
                    catch (Exception e)
                    {
                        this.HasError = true;
                        this.ErrorMessage = e.Message;
                    }
                }

                this.OnPropertyChanged(null);
            }
        }

        public bool HasError { get; private set; }

        public string ErrorMessage { get; private set; }

        public ICommand AddPackage { get; }

        public ICommand ShowAppInstallerDialog { get; }

        public ICommand ShowModificationPackageDialog { get; }

        // ReSharper disable once IdentifierTypo
        public ICommand ShowWingetDialog { get; }
        
        public ICommand ViewDependencies { get; }

        public ICommand ChangeVolume { get; }
        
        public ICommand OpenExplorer { get; }

        public ICommand OpenUserExplorer { get; }

        public ICommand OpenManifest { get; }

        public ICommand OpenConfigJson { get; }

        public ICommand OpenStore { get; }

        public ICommand CheckUpdates { get; }

        public ICommand RunTool { get; }
        
        public ICommand RemovePackage { get; }

        public ICommand Refresh { get; }

        public ICommand Copy { get; }

        public ICommand MountRegistry { get; }

        public ICommand DismountRegistry { get; }

        public ICommand StartApp { get; }

        public ICommand StopApp { get; }
        
        private async void OnStopApp()
        {
            if (this._packageDetails == null)
            {
                return;
            }

            if (this._installedPackage.SignatureKind == SignatureKind.System)
            {
                var buttons = new[]
                {
                    Resources.Localization.PackageExpert_Commands_StopSystemApp,
                    Resources.Localization.PackageExpert_Commands_LeaveRunning
                };

                if (this._interactionService.ShowMessage(Resources.Localization.PackageExpert_Commands_StopSystemApp_Remark,
                    buttons, Resources.Localization.PackageExpert_Commands_StopSystemApp_Title, systemButtons: InteractionResult.None) != 0)
                {
                    return;
                }
            }

            var executor = this._application.CommandExecutor
                .WithBusyManager(this._busyManager, OperationType.Other)
                .WithErrorHandling(this._interactionService, true);

            await executor.Invoke(this, new StopPackageCommand(this._installedPackage), CancellationToken.None).ConfigureAwait(false);
        }
        
        private bool CanStopApp()
        {
            if (this._application.ApplicationState.Packages.ActivePackageNames == null || this._installedPackage == null)
            {
                return false;
            }
            
            if (this._installedPackage.InstallLocation == null)
            {
                return false;
            }

            return this._application.ApplicationState.Packages.ActivePackageNames?.Contains(this._installedPackage.PackageFamilyName) == true;
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

                    var filterBuilder = new DialogFilterBuilder().WithWinget().WithAll();
                    this.OpenBrowseDialog(
                        ModuleNames.Dialogs.Winget,
                        NavigationPaths.DialogPaths.WingetYamlEditor,
                        "yaml",
                        filterBuilder);
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
                    var filterBuilder = new DialogFilterBuilder().WithPackages(DialogFilterBuilderPackagesExtensions.PackageTypes.Msix).WithAll();
                    this.OpenBrowseDialog(
                        ModuleNames.Dialogs.Packaging,
                        NavigationPaths.DialogPaths.PackagingModificationPackage,
                        "file",
                        filterBuilder);
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

                    var filterBuilder = new DialogFilterBuilder().WithAppInstaller().WithAll();
                    this.OpenBrowseDialog(
                        ModuleNames.Dialogs.AppInstaller,
                        NavigationPaths.DialogPaths.AppInstallerEditor,
                        "file",
                        filterBuilder);
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
            if (this._installedPackage == null)
            {
                return;
            }

            try
            {
                var manager = this._uacElevation.AsAdministrator<IRegistryManager>();
                await manager.MountRegistry(this._installedPackage, true).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                this._interactionService.ShowError(Resources.Localization.PackageExpert_Commands_MountRegistry_Error, exception);
            }
        }

        private async void OnDismountRegistry()
        {
            if (this._installedPackage == null)
            {
                return;
            }

            if (this._installedPackage.InstallLocation == null)
            {
                return;
            }

            try
            {
                var manager = this._uacElevation.AsAdministrator<IRegistryManager>();
                await manager.DismountRegistry(this._installedPackage).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                this._interactionService.ShowError(Resources.Localization.PackageExpert_Commands_DismountRegistry_Error, exception);
            }
        }
        
        private bool CanMountRegistry()
        {
            if (this._installedPackage?.InstallLocation == null)
            {
                return false;
            }
            
            try
            {
                var manager = this._uacElevation.AsCurrentUser<IRegistryManager>();
                var regState = manager.GetRegistryMountState(this._installedPackage.InstallLocation, this._installedPackage.Name).GetAwaiter().GetResult();
                return regState == RegistryMountState.NotMounted;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool CanDismountRegistry()
        {
            if (this._installedPackage?.InstallLocation == null)
            {
                return false;
            }
            
            try
            {
                var manager = this._uacElevation.AsCurrentUser<IRegistryManager>();
                var regState = manager.GetRegistryMountState(this._installedPackage.InstallLocation, this._installedPackage.Name).GetAwaiter().GetResult();
                return regState == RegistryMountState.Mounted;
            }
            catch (Exception)
            {
                return false;
            }
        }
        
        private async void OnStartApp(object parameter)
        {
            var selection = this._installedPackage;
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
                this._interactionService.ShowError(Resources.Localization.PackageExpert_Commands_StartApp_Error + " " + exception.Message, exception);
            }

            catch (Exception exception)
            {
                this._interactionService.ShowError(Resources.Localization.PackageExpert_Commands_StartApp_Error, exception);
            }
        }

        private bool CanStartApp(object parameter) => this.IsCurrentPresentAndInstalled();

        private bool CanRunTool(object parameter)
        {
            if (!(parameter is ToolListConfiguration))
            {
                return false;
            }

            return this.IsCurrentPresentAndInstalled();
        }

        private async void OnRunTool(object parameter)
        {
            if (!(parameter is ToolListConfiguration tool))
            {
                return;
            }

            var selection = this._installedPackage;
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
            var isAppInstaller = this._installedPackage.AppInstallerUri != null;
            var isStore = this._installedPackage.SignatureKind == SignatureKind.Store;

            if (!isAppInstaller && !isStore)
            {
                return;
            }

            var executor = this._application.CommandExecutor
                .WithBusyManager(this._busyManager, OperationType.Other)
                .WithErrorHandling(this._interactionService, true);

            if (isStore)
            {
                Process.Start(new ProcessStartInfo("ms-windows-store://downloadsandupdates") { UseShellExecute = true });
            }

            if (!isAppInstaller)
            {
                return;
            }

            var updateResult = await executor.Invoke<CheckForUpdatesCommand, AppInstallerUpdateAvailabilityResult>(this, new CheckForUpdatesCommand(this._installedPackage.PackageFullName)).ConfigureAwait(false);
            string msg;

            var askForUpdate = false;
            switch (updateResult)
            {
                case AppInstallerUpdateAvailabilityResult.Unknown:
                    msg = string.Format(Resources.Localization.PackageExpert_Commands_AppInstaller_WrongSource, FileConstants.AppInstallerExtension);
                    break;
                case AppInstallerUpdateAvailabilityResult.NoUpdates:
                    msg = Resources.Localization.PackageExpert_Commands_AppInstaller_WrongSource;
                    break;
                case AppInstallerUpdateAvailabilityResult.Available:
                    msg = Resources.Localization.PackageExpert_Commands_AppInstaller_OptionalUpdateAvailable;
                    askForUpdate = true;
                    break;
                case AppInstallerUpdateAvailabilityResult.Required:
                    msg = Resources.Localization.PackageExpert_Commands_AppInstaller_RequiredUpdateAvailable;
                    askForUpdate = true;
                    break;
                case AppInstallerUpdateAvailabilityResult.Error:
                    msg = Resources.Localization.PackageExpert_Commands_AppInstaller_UpdateCheckFailed;
                    break;
                default:
                    msg = Resources.Localization.PackageExpert_Commands_AppInstaller_UpdateCheckFailed;
                    break;
            }

            if (!askForUpdate)
            {
                this._interactionService.ShowInfo(msg, InteractionResult.OK, Resources.Localization.PackageExpert_Commands_AppInstaller_UpdateCheck_Result);
            }
            else
            {
                if (this._interactionService.ShowMessage(msg, new[] { Resources.Localization.PackageExpert_Commands_AppInstaller_UpdateNow }, Resources.Localization.PackageExpert_Commands_AppInstaller_UpdateCheck_Result, systemButtons: InteractionResult.Close) == 0)
                {
                    this.UpdatePackageFromAppInstaller(this._installedPackage.AppInstallerUri.ToString());
                }
            }
        }

        private void RefreshPackage()
        {
            this.Reload();
        }

        private async void InstallPackage()
        {
            AddAppxPackageOptions options = 0;

            options |= AddAppxPackageOptions.KillRunningApps;

            var context = this._busyManager.Begin();
            try
            {
                var manager = this._uacElevation.AsCurrentUser<IAppxPackageInstaller>();
                await manager.Add(this.FilePath, options, progress: context).ConfigureAwait(false);

                var appxIdentity = this._packageDetails.DisplayName;

#pragma warning disable 4014
                this._interactionService.ShowToast(Resources.Localization.PackageExpert_Commands_Add_Success1, string.Format(Resources.Localization.PackageExpert_Commands_Add_Success2, appxIdentity));
#pragma warning restore 4014
                
                this.Reload();
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

        private async void UpdatePackageFromAppInstaller(string appInstallerPath)
        {
            if (appInstallerPath == null)
            {
                return;
            }

            AddAppxPackageOptions options = 0;
            options |= AddAppxPackageOptions.KillRunningApps;

            var context = this._busyManager.Begin();
            try
            {
                var manager = this._uacElevation.AsCurrentUser<IAppxPackageInstaller>();
                await manager.Add(appInstallerPath, options, progress: context).ConfigureAwait(false);

                var _ = this._interactionService.ShowToast(Resources.Localization.PackageExpert_Commands_Add_Success1, string.Format(Resources.Localization.PackageExpert_Commands_Add_SuccessFile2, this._packageDetails.DisplayName));

                this.Reload();
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
        
        private bool CanAddPackage() => this._installedPackage == null;
        
        private bool CanRefresh() => true;

        private bool CanCheckUpdates()
        {
            return this._installedPackage?.SignatureKind == SignatureKind.Store || this._installedPackage?.AppInstallerUri != null;
        }

        private bool CanChangeVolume() => this._installedPackage?.InstallLocation != null;

        private void OnChangeVolume()
        {
            var selection = this._installedPackage;
            if (selection?.InstallLocation == null)
            {
                return;
            }

            this._prismServices.ModuleManager.LoadModule(ModuleNames.Dialogs.Volumes);
            IDialogParameters parameters = new DialogParameters();
            parameters.Add("file", this._installedPackage.InstallLocation);

            this._prismServices.DialogService.ShowDialog(NavigationPaths.DialogPaths.VolumesChangeVolume, parameters, this.OnDialogOpened);
        }

        private void OnViewDependencies()
        {
            var selection = this._installedPackage;
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

        private bool CanViewDependencies() => this.IsCurrentPresent();

        private async void OnRemovePackage(bool forAllUsers)
        {
            if (!this.IsCurrentPresentAndInstalled())
            {
                return;
            }

            var config = await this._configurationService.GetCurrentConfigurationAsync().ConfigureAwait(false);
            if (config.UiConfiguration.ConfirmDeletion)
            {
                var options = new List<string>
                {
                    forAllUsers ? Resources.Localization.PackageExpert_Commands_RemoveAllUsers : Resources.Localization.PackageExpert_Commands_RemoveCurrentUser,
                    Resources.Localization.PackageExpert_Commands_DoNotRemove
                };

                var caption = string.Format(Resources.Localization.PackageExpert_Commands_ConfirmRemovalSingle, this._installedPackage.DisplayName, this._installedPackage.Version);

                var selectedOption = this._interactionService.ShowMessage(caption, options, Resources.Localization.PackageExpert_Commands_Removing, systemButtons: InteractionResult.Cancel);
                if (selectedOption != 0)
                {
                    return;
                }
            }

            var context = this._busyManager.Begin();
            try
            {
                if (forAllUsers)
                {
                    var adminManager = this._uacElevation.AsAdministrator<IAppxPackageInstaller>();
                    await adminManager.Deprovision(this._installedPackage.PackageFamilyName, CancellationToken.None).ConfigureAwait(false);
                }

                var manager = this._uacElevation.AsCurrentUser<IAppxPackageInstaller>();
                var removedPackageName = this._installedPackage.DisplayName;

                await manager.Remove(
                    new []
                    {
                        this._installedPackage
                    }, 
                    progress: context).ConfigureAwait(false);
                
#pragma warning disable CS4014
                this._interactionService.ShowToast(Resources.Localization.PackageExpert_Commands_AppRemoved1, string.Format(Resources.Localization.PackageExpert_Commands_AppRemoved2, removedPackageName));
#pragma warning restore CS4014

                await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);

                this.Reload();
            }
            catch (Exception exception)
            {
                this._interactionService.ShowError(Resources.Localization.PackageExpert_Commands_AppRemovalFailed, exception);
            }
            finally
            {
                this._busyManager.End(context);
            }
        }

        private bool CanRemovePackage() => this.IsCurrentPresentAndInstalled();

        private bool CanOpenStore() => this._installedPackage?.SignatureKind == SignatureKind.Store;

        private bool CanOpenPsfConfig()
        {
            return this._installedPackage?.PackageType == MsixPackageType.Win32Psf;
        }

        private bool CanOpenManifest() => this.IsCurrentPresentAndInstalled();

        private void OnOpenManifest()
        {
            var package = this._installedPackage;
            if (package?.ManifestLocation == null)
            {
                return;
            }

            var config = this._configurationService.GetCurrentConfiguration().Editing ?? new EditingConfiguration();
            this._fileInvoker.Execute(config.ManifestEditorType, config.ManifestEditor, package.ManifestLocation);
        }

        private void OnOpenConfigJson()
        {
            var package = this._installedPackage;
            if (package?.PsfConfig == null)
            {
                return;
            }
            
            var config = this._configurationService.GetCurrentConfiguration().Editing ?? new EditingConfiguration();
            this._fileInvoker.Execute(config.PsfEditorType, config.PsfEditor, package.PsfConfig);
        }
        
        private bool IsCurrentPresent()
        {
            return this._packageDetails != null;
        }

        private bool IsCurrentPresentAndInstalled()
        {
            return this._packageDetails != null && this._installedPackage != null;
        }

        private void OnOpenStore()
        {
            var selection = this._installedPackage;
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

                var pkg = this._installedPackage;

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

        private bool CanOpenExplorer() => this.IsCurrentPresentAndInstalled();

        private void OnOpenExplorer()
        {
            var selection = this._installedPackage;
            if (selection?.ManifestLocation == null)
            {
                return;
            }
            
            Process.Start("explorer.exe", "/select," + selection.ManifestLocation);
        }

        private bool CanOpenUserExplorer()
        {
            var selection = this._installedPackage;
            if (selection?.PackageFamilyName == null)
            {
                return false;
            }

            var rootDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Packages", selection.PackageFamilyName, "LocalCache");
            return Directory.Exists(rootDir);
        }

        private void OnOpenUserExplorer()
        {
            var selection = this._installedPackage;
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
            Func<AppxPackage, string> valueGetter = null)
        {
            var selected = this._packageDetails;
            if (selected == null)
            {
                return;
            }
                
            var parameters = new DialogParameters
            {
                { fileParameterName, valueGetter == null ? selected.Path : valueGetter(selected) }
            };

            this._prismServices.ModuleManager.LoadModule(moduleName);
            this._prismServices.DialogService.ShowDialog(navigationPath, parameters, this.OnDialogClosed);
        }

        private void OnDialogClosed(IDialogResult obj)
        {
        }
    }
}
