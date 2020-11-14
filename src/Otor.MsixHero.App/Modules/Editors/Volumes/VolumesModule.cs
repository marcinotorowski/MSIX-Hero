using Otor.MsixHero.App.Modules.Editors.Volumes.ChangeVolume.View;
using Otor.MsixHero.App.Modules.Editors.Volumes.ChangeVolume.ViewModel;
using Otor.MsixHero.App.Modules.Editors.Volumes.NewVolume.View;
using Otor.MsixHero.App.Modules.Editors.Volumes.NewVolume.ViewModel;
using Prism.Ioc;
using Prism.Modularity;

namespace Otor.MsixHero.App.Modules.Editors.Volumes
{
    public class VolumesModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<ChangeVolumeView, ChangeVolumeViewModel>(DialogPathNames.VolumesChangeVolume);
            containerRegistry.RegisterForNavigation<NewVolumeView, NewVolumeViewModel>(DialogPathNames.VolumesNewVolume);
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}
