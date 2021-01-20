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
