using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Hero.Commands.Packages;
using Otor.MsixHero.App.Hero.Executor;
using Otor.MsixHero.Appx.Packaging.Installation;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities.Summary;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation.Enums;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Lib.Infrastructure.Progress;

namespace Otor.MsixHero.App.Modules.PackageManagement
{
    public class PackagesManagementCommandHandler
    {
        private readonly IMsixHeroApplication application;
        private readonly IInteractionService interactionService;
        private readonly ISelfElevationProxyProvider<IAppxPackageManager> packageManagerProvider;
        private readonly IBusyManager busyManager;

        public PackagesManagementCommandHandler(
            UIElement parent, 
            IMsixHeroApplication application, 
            IInteractionService interactionService,
            ISelfElevationProxyProvider<IAppxPackageManager> packageManagerProvider,
            IBusyManager busyManager)
        {
            this.application = application;
            this.interactionService = interactionService;
            this.packageManagerProvider = packageManagerProvider;
            this.busyManager = busyManager;

            parent.CommandBindings.Add(new CommandBinding(MsixHeroCommands.AddPackage, this.OnAddPackage, this.CanAddPackage));
            parent.CommandBindings.Add(new CommandBinding(NavigationCommands.Refresh, this.OnRefresh, this.CanRefresh));
        }

        private async void OnRefresh(object sender, ExecutedRoutedEventArgs e)
        {
            var executor = this.application.CommandExecutor
                .WithErrorHandling(this.interactionService, true)
                .WithBusyManager(this.busyManager, OperationType.PackageLoading);

            await executor.Invoke(this, new GetPackagesCommand(), CancellationToken.None).ConfigureAwait(false);
        }

        private void CanRefresh(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CanAddPackage(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private async void OnAddPackage(object sender, ExecutedRoutedEventArgs e)
        {
            var forAllUsers = e.Parameter is bool boolParam && boolParam;

            string packagePath;
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

    }
}
