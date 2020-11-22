using System;
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Hero.Commands.Volumes;
using Otor.MsixHero.App.Hero.Events.Base;
using Otor.MsixHero.App.Mvvm;
using Prism.Events;
using Prism.Regions;

namespace Otor.MsixHero.App.Modules.VolumeManagement.ViewModels
{
    public class VolumeManagementViewModel : NotifyPropertyChanged
    {
        private readonly IMsixHeroApplication application;
        private readonly PrismServices prismServices;
        private bool isActive;
        private bool firstRun = true;

        public VolumeManagementViewModel(
            IMsixHeroApplication application,
            PrismServices prismServices)
        {
            this.application = application;
            this.prismServices = prismServices;
            application.EventAggregator.GetEvent<UiExecutedEvent<SelectVolumesCommand>>().Subscribe(this.OnSelectVolumes, ThreadOption.UIThread);
        }

        private void OnSelectVolumes(UiExecutedPayload<SelectVolumesCommand> obj)
        {
            var parameters = new NavigationParameters
            {
                { "volumes", this.application.ApplicationState.Volumes.SelectedVolumes }
            };

            switch (this.application.ApplicationState.Volumes.SelectedVolumes.Count)
            {
                case 0:
                    this.prismServices.RegionManager.Regions[VolumeManagementRegionNames.Details].RequestNavigate(new Uri(NavigationPaths.VolumeManagementPaths.ZeroSelection, UriKind.Relative), parameters);
                    break;
                case 1:
                    this.prismServices.RegionManager.Regions[VolumeManagementRegionNames.Details].RequestNavigate(new Uri(NavigationPaths.VolumeManagementPaths.SingleSelection, UriKind.Relative), parameters);
                    break;
                default:
                    this.prismServices.RegionManager.Regions[VolumeManagementRegionNames.Details].NavigationService.RequestNavigate(new Uri(NavigationPaths.VolumeManagementPaths.MultipleSelection, UriKind.Relative), parameters);
                    break;
            }
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

                if (value && firstRun)
                {
                    firstRun = false;
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
    }
}
