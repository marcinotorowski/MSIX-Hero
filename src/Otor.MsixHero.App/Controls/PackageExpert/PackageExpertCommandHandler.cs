using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Hero.Commands.Packages;
using Otor.MsixHero.App.Hero.Executor;
using Otor.MsixHero.App.Modules.Packages.Constants;
using Otor.MsixHero.Appx.Packaging.Installation;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities.Sources;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Helpers;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation.Enums;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Lib.Infrastructure.Progress;
using Prism.Regions;

namespace Otor.MsixHero.App.Controls.PackageExpert
{
    internal class PackageExpertCommandHandler
    {
        private readonly IRegionManager regionManager;
        private readonly IMsixHeroApplication application;
        private readonly IBusyManager busyManager;
        private readonly IConfigurationService configurationService;
        private readonly IInteractionService interactionService;
        private readonly ISelfElevationProxyProvider<IAppxPackageManager> packageManagerProvider;

        public PackageExpertCommandHandler(
            UIElement parent,
            IRegionManager regionManager,
            IMsixHeroApplication application,
            IBusyManager busyManager,
            ISelfElevationProxyProvider<IAppxPackageManager> packageManagerProvider,
            IConfigurationService configurationService,
            IInteractionService interactionService,
            FileInvoker fileInvoker)
        {
            this.regionManager = regionManager;
            this.application = application;
            this.busyManager = busyManager;
            this.configurationService = configurationService;
            this.interactionService = interactionService;
            this.fileInvoker = fileInvoker;
            this.packageManagerProvider = packageManagerProvider;

            parent.CommandBindings.Add(new CommandBinding(MsixHeroCommands.OpenExplorer, this.OnOpenExplorer, this.CanOpenExplorer));
            parent.CommandBindings.Add(new CommandBinding(MsixHeroCommands.OpenExplorerUser, this.OnOpenUser, this.CanOpenUser));
            parent.CommandBindings.Add(new CommandBinding(MsixHeroCommands.OpenManifest, this.OnOpenManifest, this.CanOpenManifest));
            parent.CommandBindings.Add(new CommandBinding(MsixHeroCommands.OpenConfigJson, this.OnOpenConfigJson, this.CanOpenPsfConfig));
            parent.CommandBindings.Add(new CommandBinding(MsixHeroCommands.OpenStore, this.OnOpenStore, this.CanOpenStore));
            parent.CommandBindings.Add(new CommandBinding(MsixHeroCommands.CheckUpdates, this.OnCheckUpdates, this.CanCheckUpdates));
            parent.CommandBindings.Add(new CommandBinding(MsixHeroCommands.RunTool, this.OnRunTool, this.CanRunTool));
            parent.CommandBindings.Add(new CommandBinding(MsixHeroCommands.RunPackage, this.OnRunPackage, this.CanRunPackage));
            parent.CommandBindings.Add(new CommandBinding(MsixHeroCommands.RemovePackage, this.OnRemovePackage, this.CanRemovePackage));
            parent.CommandBindings.Add(new CommandBinding(ApplicationCommands.Copy, this.OnCopy, this.CanCopy));
        }

        private async void OnRunPackage(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var manager = await this.packageManagerProvider.GetProxyFor().ConfigureAwait(false);
                await manager.Run(Path.Combine(this.Package.RootFolder, "appxmanifest.xml"), (string)e.Parameter).ConfigureAwait(false);
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

        private void CanRunPackage(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = e.Parameter != null;
        }

        private void CanRunTool(object sender, CanExecuteRoutedEventArgs e)
        {
            if (!(e.Parameter is ToolListConfiguration))
            {
                return;
            }

            if (!this.Package.Applications.Any())
            {
                return;
            }

            e.CanExecute = true;
        }

        private async void OnRunTool(object sender, ExecutedRoutedEventArgs e)
        {
            if (!(e.Parameter is ToolListConfiguration tool))
            {
                return;
            }

            if (!this.Package.Applications.Any())
            {
                return;
            }

            var context = this.busyManager.Begin();
            try
            {
                var manager = await this.packageManagerProvider.GetProxyFor(tool.AsAdmin ? SelfElevationLevel.AsAdministrator : SelfElevationLevel.AsInvoker).ConfigureAwait(false);
                await manager.RunToolInContext(this.Package.FamilyName, this.Package.Applications[0].Id, tool.Path, tool.Arguments, CancellationToken.None, context).ConfigureAwait(false);
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

        private readonly FileInvoker fileInvoker;

        public AppxPackage Package { get; set; }

        private async void OnCheckUpdates(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.Package == null)
            {
                return;
            }

            if (this.Package.Source is StorePackageSource)
            {
                Process.Start(new ProcessStartInfo("ms-windows-store://downloadsandupdates") { UseShellExecute = true });
            }
            else if (this.Package.Source is AppInstallerPackageSource appInstaller)
            {
                var executor = this.application.CommandExecutor
                    .WithBusyManager(this.busyManager, OperationType.Other)
                    .WithErrorHandling(this.interactionService, true);

                var updateResult = await executor.Invoke<CheckForUpdatesCommand, AppInstallerUpdateAvailabilityResult>(this, new CheckForUpdatesCommand(this.Package.FullName)).ConfigureAwait(false);
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
                        var manager = await this.packageManagerProvider.GetProxyFor().ConfigureAwait(false);
                        await manager.Add(appInstaller.AppInstallerUri.ToString(), AddPackageOptions.KillRunningApps).ConfigureAwait(false);
                        await this.application.CommandExecutor.Invoke(this, new GetPackagesCommand()).ConfigureAwait(false);
                        await this.application.CommandExecutor.Invoke(this, new SelectPackagesCommand(Path.Combine(this.Package.RootFolder, "appxmanifest.xml"))).ConfigureAwait(false);
                    }
                }
            }
        }

        private void CanCheckUpdates(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.Package?.Source is AppInstallerPackageSource || this.Package?.Source is StorePackageSource;
        }

        private async void OnRemovePackage(object sender, ExecutedRoutedEventArgs e)
        {
            var config = await this.configurationService.GetCurrentConfigurationAsync().ConfigureAwait(false);
            if (config.UiConfiguration.ConfirmDeletion)
            {
                var options = new List<string>
                {
                    "Remove for current user",
                    "Do not remove"
                };

                var caption = "Are you sure you want to remove " + this.Package.DisplayName + " " + this.Package.Version + "? This operation is irreversible.";

                var selectedOption = this.interactionService.ShowMessage(caption, options, "Removing package", systemButtons: InteractionResult.Cancel);
                if (selectedOption != 0)
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

                    var manager = await this.packageManagerProvider.GetProxyFor().ConfigureAwait(false);
                    await manager.Remove(new [] { this.Package.FullName }, progress: p1).ConfigureAwait(false);

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

        private void CanRemovePackage(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.Package != null;
        }

        private void CanOpenStore(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.Package?.Source is StorePackageSource;
        }

        private void CanOpenPsfConfig(object sender, CanExecuteRoutedEventArgs e)
        {
            var rootDir = this.Package?.RootFolder;
            if (rootDir == null)
            {
                return;
            }

            e.CanExecute = File.Exists(Path.Combine(rootDir, "config.json"));
        }

        private void CanOpenManifest(object sender, CanExecuteRoutedEventArgs e)
        {
            var rootDir = this.Package?.RootFolder;
            if (rootDir == null)
            {
                return;
            }

            e.CanExecute = File.Exists(Path.Combine(rootDir, "appxmanifest.xml"));
        }

        private void OnOpenManifest(object sender, ExecutedRoutedEventArgs e)
        {
            var rootDir = this.Package?.RootFolder;
            if (rootDir == null)
            {
                return;
            }

            var path = Path.Combine(rootDir, "appxmanifest.xml");

            var config = this.configurationService.GetCurrentConfiguration().Editing ?? new EditingConfiguration();
            this.fileInvoker.Execute(config.ManifestEditorType, config.ManifestEditor, path);
        }

        private void OnOpenConfigJson(object sender, ExecutedRoutedEventArgs e)
        {
            var rootDir = this.Package?.RootFolder;
            if (rootDir == null)
            {
                return;
            }

            var path = Path.Combine(rootDir, "config.json");

            var config = this.configurationService.GetCurrentConfiguration().Editing ?? new EditingConfiguration();
            this.fileInvoker.Execute(config.PsfEditorType, config.PsfEditor, path);
        }

        private void OnOpenStore(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.Package.Source is StorePackageSource store)
            {
                var link = $"ms-windows-store://pdp/?PFN={store.FamilyName}";
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
        }

        private void OnCopy(object sender, ExecutedRoutedEventArgs e)
        {
            Clipboard.SetText((string)e.Parameter);
        }

        private void CanCopy(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = e.Parameter != null;
        }

        private void CanOpenExplorer(object sender, CanExecuteRoutedEventArgs e)
        {
            var rootDir = this.Package?.RootFolder;
            if (rootDir == null)
            {
                return;
            }

            var path = Path.Combine(rootDir, "appxmanifest.xml");
            e.CanExecute = File.Exists(path);
        }

        private void OnOpenExplorer(object sender, ExecutedRoutedEventArgs e)
        {
            var rootDir = this.Package?.RootFolder;
            if (rootDir == null)
            {
                return;
            }

            var path = Path.Combine(rootDir, "appxmanifest.xml");

            // ReSharper disable once AssignNullToNotNullAttribute
            // ReSharper disable once StringLiteralTypo
            Process.Start("explorer.exe", "/select," + path);
        }

        private void CanOpenUser(object sender, CanExecuteRoutedEventArgs e)
        {
            if (this.Package?.FamilyName == null)
            {
                return;
            }

            var rootDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Packages", this.Package.FamilyName, "LocalCache");
            e.CanExecute = Directory.Exists(rootDir);
        }

        private void OnOpenUser(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.Package == null)
            {
                return;
            }

            var rootDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Packages", this.Package.FamilyName, "LocalCache");
            Process.Start("explorer.exe", "/e," + rootDir);
        }
    }
}
