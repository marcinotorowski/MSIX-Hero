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
using System.Collections.Generic;
using NUnit.Framework;
using Otor.MsixHero.AppInstaller;
using Otor.MsixHero.AppInstaller.Entities;

namespace Otor.MsixHero.Tests.Appx.AppInstaller
{
    [TestFixture]
    public class FeatureSupportTests
    {
        [Test]
        public void TestVersions()
        {
            var appInstaller = GetConfig();
            var supportHelper = new AppInstallerFeatureSupportHelper();
            Assert.That(supportHelper.GetLowestSupportedWindows10Build(appInstaller), Is.EqualTo(1709));

            // AutomaticBackgroundTask should be supported from 1803 onward
            appInstaller = GetConfig(c => c.UpdateSettings = new UpdateSettings
            {
                AutomaticBackgroundTask = new AutomaticBackgroundTaskSettings()
            });
            Assert.That(supportHelper.GetLowestSupportedWindows10Build(appInstaller), Is.EqualTo(1803));

            // ForceUpdateFromAnyVersion should be supported from 1809 onward
            appInstaller = GetConfig(c => c.UpdateSettings = new UpdateSettings
            {
                ForceUpdateFromAnyVersion = true
            });
            Assert.That(supportHelper.GetLowestSupportedWindows10Build(appInstaller), Is.EqualTo(1809));
            appInstaller = GetConfig(c => c.UpdateSettings = new UpdateSettings
            {
                ForceUpdateFromAnyVersion = false
            });
            Assert.That(supportHelper.GetLowestSupportedWindows10Build(appInstaller), Is.EqualTo(1709));

            // UpdateBlocksActivation should be supported from 1903 onward
            appInstaller = GetConfig(c => c.UpdateSettings = new UpdateSettings
            {
                OnLaunch = new OnLaunchSettings
                {
                    UpdateBlocksActivation = true
                }
            });
            Assert.That(supportHelper.GetLowestSupportedWindows10Build(appInstaller), Is.EqualTo(1903));
            
            appInstaller = GetConfig(c => c.UpdateSettings = new UpdateSettings
            {
                OnLaunch = new OnLaunchSettings
                {
                    UpdateBlocksActivation = false
                }
            });
            Assert.That(supportHelper.GetLowestSupportedWindows10Build(appInstaller), Is.EqualTo(1709));

            // ShowPrompt should be supported from 1903 onward
            appInstaller = GetConfig(c => c.UpdateSettings = new UpdateSettings
            {
                OnLaunch = new OnLaunchSettings
                {
                    ShowPrompt = true
                }
            });
            Assert.That(supportHelper.GetLowestSupportedWindows10Build(appInstaller), Is.EqualTo(1903));
            
            appInstaller = GetConfig(c => c.UpdateSettings = new UpdateSettings
            {
                OnLaunch = new OnLaunchSettings
                {
                    ShowPrompt = false
                }
            });
            Assert.That(supportHelper.GetLowestSupportedWindows10Build(appInstaller), Is.EqualTo(1709));
        }

        [Test]
        public void TestNamespaces()
        {
            var appInstaller = GetConfig();
            var supportHelper = new AppInstallerFeatureSupportHelper();
            Assert.That(supportHelper.GetLowestCommonNamespace(appInstaller), Is.EqualTo("http://schemas.microsoft.com/appx/appinstaller/2017"));

            // AutomaticBackgroundTask should be supported from 1803 onward
            appInstaller = GetConfig(c => c.UpdateSettings = new UpdateSettings
            {
                AutomaticBackgroundTask = new AutomaticBackgroundTaskSettings()
            });
            Assert.That(supportHelper.GetLowestCommonNamespace(appInstaller), Is.EqualTo("http://schemas.microsoft.com/appx/appinstaller/2017/2"));

            // ForceUpdateFromAnyVersion should be supported from 1809 onward
            appInstaller = GetConfig(c => c.UpdateSettings = new UpdateSettings
            {
                ForceUpdateFromAnyVersion = true
            });
            Assert.That(supportHelper.GetLowestCommonNamespace(appInstaller), Is.EqualTo("http://schemas.microsoft.com/appx/appinstaller/2017/2"));
            appInstaller = GetConfig(c => c.UpdateSettings = new UpdateSettings
            {
                ForceUpdateFromAnyVersion = false
            });
            Assert.That(supportHelper.GetLowestCommonNamespace(appInstaller), Is.EqualTo("http://schemas.microsoft.com/appx/appinstaller/2017"));

            // UpdateBlocksActivation should be supported from 1903 onward
            appInstaller = GetConfig(c => c.UpdateSettings = new UpdateSettings
            {
                OnLaunch = new OnLaunchSettings
                {
                    UpdateBlocksActivation = true
                }
            });
            Assert.That(supportHelper.GetLowestCommonNamespace(appInstaller), Is.EqualTo("http://schemas.microsoft.com/appx/appinstaller/2018"));

            appInstaller = GetConfig(c => c.UpdateSettings = new UpdateSettings
            {
                OnLaunch = new OnLaunchSettings
                {
                    UpdateBlocksActivation = false
                }
            });
            Assert.That(supportHelper.GetLowestCommonNamespace(appInstaller), Is.EqualTo("http://schemas.microsoft.com/appx/appinstaller/2017"));

            // ShowPrompt should be supported from 1903 onward
            appInstaller = GetConfig(c => c.UpdateSettings = new UpdateSettings
            {
                OnLaunch = new OnLaunchSettings
                {
                    ShowPrompt = true
                }
            });
            Assert.That(supportHelper.GetLowestCommonNamespace(appInstaller), Is.EqualTo("http://schemas.microsoft.com/appx/appinstaller/2018"));

            appInstaller = GetConfig(c => c.UpdateSettings = new UpdateSettings
            {
                OnLaunch = new OnLaunchSettings
                {
                    ShowPrompt = false
                }
            });
            Assert.That(supportHelper.GetLowestCommonNamespace(appInstaller), Is.EqualTo("http://schemas.microsoft.com/appx/appinstaller/2017"));
        }

        private static AppInstallerConfig GetConfig(Action<AppInstallerConfig> configurator = null)
        {
            var newConfig = new AppInstallerConfig();
            configurator?.Invoke(newConfig);
            return newConfig;
        }
    }
}
