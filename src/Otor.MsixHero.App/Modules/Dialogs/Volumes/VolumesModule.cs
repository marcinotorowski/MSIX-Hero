using Otor.MsixHero.App.Controls;
using Otor.MsixHero.App.Modules.Dialogs.Volumes.ChangeVolume.View;
using Otor.MsixHero.App.Modules.Dialogs.Volumes.ChangeVolume.ViewModel;
using Otor.MsixHero.App.Modules.Dialogs.Volumes.NewVolume.View;
using Otor.MsixHero.App.Modules.Dialogs.Volumes.NewVolume.ViewModel;
using Prism.Ioc;
using Prism.Modularity;

namespace Otor.MsixHero.App.Modules.Dialogs.Volumes
{
    public class VolumesModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterDialog<ChangeVolumeView, ChangeVolumeViewModel>(NavigationPaths.DialogPaths.VolumesChangeVolume);
            containerRegistry.RegisterDialog<NewVolumeView, NewVolumeViewModel>(NavigationPaths.DialogPaths.VolumesNewVolume);
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}
