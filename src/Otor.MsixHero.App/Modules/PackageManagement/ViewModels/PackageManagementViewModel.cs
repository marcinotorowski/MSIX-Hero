using System;
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Hero.Commands.Packages;
using Otor.MsixHero.App.Hero.Events.Base;
using Otor.MsixHero.App.Modules.PackageManagement.Commands;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Diagnostic.Registry;
using Otor.MsixHero.Appx.Packaging.Installation;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Lib.Infrastructure.Progress;
using Prism.Events;
using Prism.Regions;

namespace Otor.MsixHero.App.Modules.PackageManagement.ViewModels
{
    public class PackageManagementViewModel : NotifyPropertyChanged
    {
        private readonly IMsixHeroApplication application;
        private readonly PrismServices prismServices;

        public PackageManagementViewModel(
            IMsixHeroApplication application,
            IInteractionService interactionService,
            ISelfElevationProxyProvider<IAppxPackageManager> packageManagerProvider,
            ISelfElevationProxyProvider<IRegistryManager> registryManagerProvider,
            PrismServices prismServices,
            IBusyManager busyManager, 
            IConfigurationService configurationService)
        {
            this.application = application;
            this.prismServices = prismServices;
            this.CommandHandler = new PackagesManagementCommandHandler(
                application, 
                interactionService, 
                configurationService,
                prismServices,
                packageManagerProvider, 
                registryManagerProvider,
                busyManager);

            application.EventAggregator.GetEvent<UiExecutedEvent<SelectPackagesCommand>>().Subscribe(this.OnSelectPackages, ThreadOption.UIThread);
        }

        private void OnSelectPackages(UiExecutedPayload<SelectPackagesCommand> obj)
        {
            var parameters = new NavigationParameters
            {
                { "packages", this.application.ApplicationState.Packages.SelectedPackages }
            };

            switch (this.application.ApplicationState.Packages.SelectedPackages.Count)
            {
                case 0:
                    this.prismServices.RegionManager.Regions[PackageManagementRegionNames.Details].RequestNavigate(new Uri(NavigationPaths.PackageManagementPaths.ZeroSelection, UriKind.Relative), parameters);
                    break;
                case 1:
                    this.prismServices.RegionManager.Regions[PackageManagementRegionNames.Details].RequestNavigate(new Uri(NavigationPaths.PackageManagementPaths.SingleSelection, UriKind.Relative), parameters);
                    break;
                default:
                    this.prismServices.RegionManager.Regions[PackageManagementRegionNames.Details].NavigationService.RequestNavigate(new Uri(NavigationPaths.PackageManagementPaths.MultipleSelection, UriKind.Relative), parameters);
                    break;
            }
        }

        public PackagesManagementCommandHandler CommandHandler { get; }
    }
}
