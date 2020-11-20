using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Modules.PackageManagement.Commands;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Packaging.Installation;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Lib.Infrastructure.Progress;

namespace Otor.MsixHero.App.Modules.PackageManagement.ViewModels
{
    public class PackageManagementViewModel : NotifyPropertyChanged
    {
        public PackageManagementViewModel(
            IMsixHeroApplication application,
            IInteractionService interactionService,
            ISelfElevationProxyProvider<IAppxPackageManager> packageManagerProvider,
            PrismServices prismServices,
            IBusyManager busyManager, 
            IConfigurationService configurationService)
        {
            this.CommandHandler = new PackagesManagementCommandHandler(
                application, 
                interactionService, 
                configurationService,
                prismServices,
                packageManagerProvider, 
                busyManager);
        }

        public PackagesManagementCommandHandler CommandHandler { get; }
    }
}
