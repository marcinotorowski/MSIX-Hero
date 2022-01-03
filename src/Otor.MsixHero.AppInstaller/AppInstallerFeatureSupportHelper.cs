// MSIX Hero
// Copyright (C) 2022 Marcin Otorowski
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

using System;
using Otor.MsixHero.AppInstaller.Entities;

namespace Otor.MsixHero.AppInstaller
{
    public class AppInstallerFeatureSupportHelper
    {
        public string GetLowestCommonNamespace(AppInstallerConfig config)
        {
            var maximumNamespace = 20170;

            if (config.UpdateSettings != null)
            {
                if (config.UpdateSettings.ForceUpdateFromAnyVersion)
                {
                    maximumNamespace = Math.Max(maximumNamespace, 20172);
                }

                if (config.UpdateSettings.AutomaticBackgroundTask != null)
                {
                    maximumNamespace = Math.Max(maximumNamespace, 20172);
                }

                if (config.UpdateSettings.OnLaunch != null)
                {
                    if (config.UpdateSettings.OnLaunch.UpdateBlocksActivation || config.UpdateSettings.OnLaunch.ShowPrompt)
                    {
                        maximumNamespace = Math.Max(maximumNamespace, 20180);
                    }
                }
            }

            var minimumNamespace = "http://schemas.microsoft.com/appx/appinstaller/";
            if (maximumNamespace % 10 == 0)
            {
                minimumNamespace += (maximumNamespace / 10);
            }
            else
            {
                var minor = maximumNamespace % 10;
                var major = (maximumNamespace - minor) / 10;

                minimumNamespace += major;
                if (minor != 0)
                {
                    minimumNamespace += "/" + minor;
                }
            }

            return minimumNamespace;
        }
        
        public int GetLowestSupportedWindows10Build(AppInstallerConfig config)
        {
            var windowsBuild = 1709;

            if (config.UpdateSettings != null)
            {
                if (config.UpdateSettings.ForceUpdateFromAnyVersion)
                {
                    windowsBuild = Math.Max(windowsBuild, 1809);
                }

                if (config.UpdateSettings.AutomaticBackgroundTask != null)
                {
                    windowsBuild = Math.Max(windowsBuild, 1803);
                }

                if (config.UpdateSettings.OnLaunch != null)
                {
                    if (config.UpdateSettings.OnLaunch.UpdateBlocksActivation || config.UpdateSettings.OnLaunch.ShowPrompt)
                    {
                        windowsBuild = Math.Max(windowsBuild, 1903);
                    }
                }
            }

            return windowsBuild;
        }
    }
}
