using Prism.Modularity;
using Prism.Regions;
using Prism.Services.Dialogs;

namespace Otor.MsixHero.App.Hero
{
    public class PrismServices
    {
        public PrismServices(
            IRegionManager regionManager, 
            IRegionNavigationService navigationService, 
            IModuleManager moduleManager, 
            IDialogService dialogService)
        {
            this.RegionManager = regionManager;
            this.NavigationService = navigationService;
            this.ModuleManager = moduleManager;
            this.DialogService = dialogService;
        }

        public IRegionManager RegionManager { get; }

        public IRegionNavigationService NavigationService { get; }

        public IModuleManager ModuleManager { get; }

        public IDialogService DialogService { get; }
    }
}
