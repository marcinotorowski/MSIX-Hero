using MediatR;
using Otor.MsixHero.App.Hero.Commands;
using Otor.MsixHero.App.Hero.Executor;
using Otor.MsixHero.App.Hero.State;
using Otor.MsixHero.App.Modules;
using Prism.Modularity;
using Prism.Regions;

namespace Otor.MsixHero.App.Hero.Handlers
{
    public class SetCurrentModeHandler : RequestHandler<SetCurrentModeCommand>
    {
        private readonly IMsixHeroCommandExecutor commandExecutor;
        private readonly IModuleManager moduleManager;
        private readonly IRegionManager regionManager;

        public SetCurrentModeHandler(
            IMsixHeroCommandExecutor commandExecutor,
            IModuleManager moduleManager,
            IRegionManager regionManager)
        {
            this.commandExecutor = commandExecutor;
            this.moduleManager = moduleManager;
            this.regionManager = regionManager;
        }

        protected override void Handle(SetCurrentModeCommand request)
        {
            this.commandExecutor.ApplicationState.CurrentMode = request.NewMode;

            switch (request.NewMode)
            {
                case ApplicationMode.Packages:
                    this.moduleManager.LoadModule(ModuleNames.PackageManagement);
                    this.regionManager.Regions[RegionNames.Main].RequestNavigate(NavigationPaths.PackageManagement);
                    this.regionManager.Regions[RegionNames.Search].RequestNavigate(NavigationPaths.PackageManagementPaths.Search);
                    break;
                case ApplicationMode.Tools:
                    this.moduleManager.LoadModule(ModuleNames.Tools);
                    this.regionManager.Regions[RegionNames.Main].RequestNavigate(NavigationPaths.Tools);
                    this.regionManager.Regions[RegionNames.Search].RequestNavigate(NavigationPaths.ToolsPaths.Search);
                    break;
                case ApplicationMode.WhatsNew:
                    this.moduleManager.LoadModule(ModuleNames.WhatsNew);
                    this.regionManager.Regions[RegionNames.Main].RequestNavigate(NavigationPaths.WhatsNew);
                    this.regionManager.Regions[RegionNames.Search].RequestNavigate(NavigationPaths.Empty);
                    break;
                case ApplicationMode.VolumeManager:
                    this.moduleManager.LoadModule(ModuleNames.VolumeManagement);
                    this.regionManager.Regions[RegionNames.Main].RequestNavigate(NavigationPaths.VolumeManagement);
                    this.regionManager.Regions[RegionNames.Search].RequestNavigate(NavigationPaths.VolumeManagementPaths.Search);
                    break;
                case ApplicationMode.SystemStatus:
                    this.moduleManager.LoadModule(ModuleNames.SystemStatus);
                    this.regionManager.Regions[RegionNames.Main].RequestNavigate(NavigationPaths.SystemStatus);
                    this.regionManager.Regions[RegionNames.Search].RequestNavigate(NavigationPaths.Empty);
                    break;
                case ApplicationMode.EventViewer:
                    this.moduleManager.LoadModule(ModuleNames.EventViewer);
                    this.regionManager.Regions[RegionNames.Main].RequestNavigate(NavigationPaths.EventViewer);
                    this.regionManager.Regions[RegionNames.Search].RequestNavigate(NavigationPaths.EventViewerPaths.Search);
                    break;
            }
        }
    }
}