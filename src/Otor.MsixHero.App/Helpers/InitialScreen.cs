using System.Threading.Tasks;
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Hero.Commands;
using Otor.MsixHero.App.Hero.State;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Services;

namespace Otor.MsixHero.App.Helpers
{
    public class InitialScreen
    {
        private readonly IMsixHeroApplication application;
        private readonly IConfigurationService configurationService;

        public InitialScreen(IMsixHeroApplication application, IConfigurationService configurationService)
        {
            this.application = application;
            this.configurationService = configurationService;
        }

        public Task GoToDefaultScreenAsync()
        {
            var cfg = this.configurationService.GetCurrentConfiguration().UiConfiguration;
            if (cfg == null)
            {
                return this.application.CommandExecutor.Invoke(this, this.GetGoToAction(DefaultScreen.Packages));
            }

            return this.application.CommandExecutor.Invoke(this, this.GetGoToAction(cfg.DefaultScreen));
        }

        public void GoToDefaultScreen()
        {
            var cfg = this.configurationService.GetCurrentConfiguration().UiConfiguration;
            if (cfg == null)
            { 
                this.application.CommandExecutor.Invoke(this, this.GetGoToAction(DefaultScreen.Packages)).GetAwaiter().GetResult();
            }
            else
            {
                this.application.CommandExecutor.Invoke(this, this.GetGoToAction(cfg.DefaultScreen)).GetAwaiter().GetResult();
            }
        }

        private SetCurrentModeCommand GetGoToAction(DefaultScreen defaultScreen)
        {
            switch (defaultScreen)
            {
                case DefaultScreen.Packages:
                    return new SetCurrentModeCommand(ApplicationMode.Packages);
                case DefaultScreen.Volumes:
                    return new SetCurrentModeCommand(ApplicationMode.VolumeManager);
                case DefaultScreen.Events:
                    return new SetCurrentModeCommand(ApplicationMode.EventViewer);
                case DefaultScreen.System:
                    return new SetCurrentModeCommand(ApplicationMode.SystemStatus);
                case DefaultScreen.Dashboard:
                    return new SetCurrentModeCommand(ApplicationMode.Dashboard);
                default:
                    return new SetCurrentModeCommand(ApplicationMode.Packages);
            }
        }
    }
}
