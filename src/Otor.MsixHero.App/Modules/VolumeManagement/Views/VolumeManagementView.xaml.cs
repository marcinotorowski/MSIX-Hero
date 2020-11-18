using System.Windows;
using Otor.MsixHero.App.Events;
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.Appx.Volumes;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Lib.Infrastructure.Progress;
using Prism.Modularity;
using Prism.Regions;
using Prism.Services.Dialogs;

namespace Otor.MsixHero.App.Modules.VolumeManagement.Views
{
    public partial class VolumeManagementView : INavigationAware
    {
        private readonly IMsixHeroApplication application;

        // ReSharper disable once NotAccessedField.Local
        private VolumeManagementHandler commandHandler;

        public VolumeManagementView(
            IMsixHeroApplication application,
            IInteractionService interactionService, 
            IConfigurationService configurationService, 
            ISelfElevationProxyProvider<IAppxVolumeManager> volumeManagerProvider,
            IBusyManager busyManager,
            IDialogService dialogService, 
            IModuleManager moduleManager)
        {
            this.application = application;
            this.InitializeComponent();
            this.commandHandler = new VolumeManagementHandler(
                this, 
                application, 
                interactionService, 
                configurationService,
                volumeManagerProvider, 
                busyManager, 
                dialogService,
                moduleManager);
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            this.application.EventAggregator.GetEvent<TopSearchWidthChangeEvent>().Publish(new TopSearchWidthChangeEventPayLoad(this.Region.ActualWidth));
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        private void RegionOnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.application.EventAggregator.GetEvent<TopSearchWidthChangeEvent>().Publish(new TopSearchWidthChangeEventPayLoad(this.Region.ActualWidth));
        }
    }
}
