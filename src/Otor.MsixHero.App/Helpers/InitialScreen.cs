// MSIX Hero
// Copyright (C) 2021 Marcin Otorowski
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// Full notice:
// https://github.com/marcinotorowski/msix-hero/blob/develop/LICENSE.md

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

        // ReSharper disable once UnusedMember.Global
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
