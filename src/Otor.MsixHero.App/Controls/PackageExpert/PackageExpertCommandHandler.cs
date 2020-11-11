using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Otor.MsixHero.App.Commands;
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Hero.Commands.Packages;
using Otor.MsixHero.App.Hero.Executor;
using Otor.MsixHero.Appx.Packaging.Installation;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities.Sources;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Helpers;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation.Enums;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Lib.Infrastructure.Progress;

namespace Otor.MsixHero.App.Controls.PackageExpert
{
    internal class PackageExpertCommandHandler
    {
        private readonly IMsixHeroApplication application;
        private readonly IBusyManager busyManager;
        private readonly IConfigurationService configurationService;
        private readonly IInteractionService interactionService;
        private readonly ISelfElevationProxyProvider<IAppxPackageManager> packageManagerProvider;

        public PackageExpertCommandHandler(
            UIElement parent,
            IMsixHeroApplication application,
            IBusyManager busyManager,
            ISelfElevationProxyProvider<IAppxPackageManager> packageManagerProvider,
            IConfigurationService configurationService,
            IInteractionService interactionService,
            FileInvoker fileInvoker)
        {
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
            parent.CommandBindings.Add(new CommandBinding(ApplicationCommands.Copy, this.OnCopy, this.CanCopy));
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

                        await this.application.CommandExecutor.Invoke(this,
                                new SelectPackagesCommand(Path.Combine(this.Package.RootFolder, "appxmanifest.xml")))
                            .ConfigureAwait(false);
                    }
                }
            }
        }

        private void CanCheckUpdates(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.Package?.Source is AppInstallerPackageSource || this.Package?.Source is StorePackageSource;
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
            var rootDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Packages", this.Package.FamilyName, "LocalCache");
            e.CanExecute = Directory.Exists(rootDir);
        }

        private void OnOpenUser(object sender, ExecutedRoutedEventArgs e)
        {
            var rootDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Packages", this.Package.FamilyName, "LocalCache");
            Process.Start("explorer.exe", "/e," + rootDir);
        }
    }
}
