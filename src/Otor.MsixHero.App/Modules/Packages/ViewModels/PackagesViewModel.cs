using System;
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Hero.Commands.Packages;
using Otor.MsixHero.App.Hero.Events.Base;
using Otor.MsixHero.App.Modules.Packages.Constants;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Packaging.Installation;
using Otor.MsixHero.Infrastructure.Services;
using Prism;
using Prism.Regions;

namespace Otor.MsixHero.App.Modules.Packages.ViewModels
{
    public class PackagesViewModel : NotifyPropertyChanged, IActiveAware, INavigationAware
    {
        private readonly IMsixHeroApplication msixHeroApp;
        private readonly IInteractionService interactionService;
        private readonly IRegionManager regionManager;
        private bool isActive;
        private bool firstRun = true;

        public PackagesViewModel(
            IMsixHeroApplication msixHeroApp,
            IInteractionService interactionService,
            IRegionManager regionManager)
        {
            this.msixHeroApp = msixHeroApp;
            this.interactionService = interactionService;
            this.regionManager = regionManager;
            this.msixHeroApp.EventAggregator.GetEvent<UiExecutedEvent<GetPackagesCommand>>().Subscribe(this.OnGetPackages);
        }

        public bool IsActive
        {
            get => this.isActive;
            set
            {
                if (!this.SetField(ref this.isActive, value))
                {
                    return;
                }

                if (value)
                {
                    this.regionManager.Regions[RegionNames.Search].RequestNavigate(PackagesNavigationPaths.Search);

                    if (firstRun)
                    {
                        firstRun = false;
                        this.msixHeroApp.CommandExecutor.Invoke(this, new GetPackagesCommand(PackageFindMode.Auto));
                    }
                }

                this.IsActiveChanged?.Invoke(this, new EventArgs());
            }
        }

        public event EventHandler IsActiveChanged;

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }
        private void OnGetPackages(UiExecutedPayload<GetPackagesCommand> obj)
        {
            this.interactionService.ShowInfo("Packages loaded (" + this.msixHeroApp.ApplicationState.Packages.AllPackages.Count + ")");
        }
    }
}
