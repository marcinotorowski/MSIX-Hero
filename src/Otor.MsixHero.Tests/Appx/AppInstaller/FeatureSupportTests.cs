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
            Assert.AreEqual(1709, supportHelper.GetLowestSupportedWindows10Build(appInstaller));

            // AutomaticBackgroundTask should be supported from 1803 onward
            appInstaller = GetConfig(c => c.UpdateSettings = new UpdateSettings
            {
                AutomaticBackgroundTask = new AutomaticBackgroundTaskSettings()
            });
            Assert.AreEqual(1803, supportHelper.GetLowestSupportedWindows10Build(appInstaller));

            // ForceUpdateFromAnyVersion should be supported from 1809 onward
            appInstaller = GetConfig(c => c.UpdateSettings = new UpdateSettings
            {
                ForceUpdateFromAnyVersion = true
            });
            Assert.AreEqual(1809, supportHelper.GetLowestSupportedWindows10Build(appInstaller));
            appInstaller = GetConfig(c => c.UpdateSettings = new UpdateSettings
            {
                ForceUpdateFromAnyVersion = false
            });
            Assert.AreEqual(1709, supportHelper.GetLowestSupportedWindows10Build(appInstaller));

            // UpdateBlocksActivation should be supported from 1903 onward
            appInstaller = GetConfig(c => c.UpdateSettings = new UpdateSettings
            {
                OnLaunch = new OnLaunchSettings
                {
                    UpdateBlocksActivation = true
                }
            });
            Assert.AreEqual(1903, supportHelper.GetLowestSupportedWindows10Build(appInstaller));
            
            appInstaller = GetConfig(c => c.UpdateSettings = new UpdateSettings
            {
                OnLaunch = new OnLaunchSettings
                {
                    UpdateBlocksActivation = false
                }
            });
            Assert.AreEqual(1709, supportHelper.GetLowestSupportedWindows10Build(appInstaller));

            // ShowPrompt should be supported from 1903 onward
            appInstaller = GetConfig(c => c.UpdateSettings = new UpdateSettings
            {
                OnLaunch = new OnLaunchSettings
                {
                    ShowPrompt = true
                }
            });
            Assert.AreEqual(1903, supportHelper.GetLowestSupportedWindows10Build(appInstaller));
            
            appInstaller = GetConfig(c => c.UpdateSettings = new UpdateSettings
            {
                OnLaunch = new OnLaunchSettings
                {
                    ShowPrompt = false
                }
            });
            Assert.AreEqual(1709, supportHelper.GetLowestSupportedWindows10Build(appInstaller));
        }

        [Test]
        public void TestNamespaces()
        {
            var appInstaller = GetConfig();
            var supportHelper = new AppInstallerFeatureSupportHelper();
            Assert.AreEqual("http://schemas.microsoft.com/appx/appinstaller/2017", supportHelper.GetLowestCommonNamespace(appInstaller));

            // AutomaticBackgroundTask should be supported from 1803 onward
            appInstaller = GetConfig(c => c.UpdateSettings = new UpdateSettings
            {
                AutomaticBackgroundTask = new AutomaticBackgroundTaskSettings()
            });
            Assert.AreEqual("http://schemas.microsoft.com/appx/appinstaller/2017/2", supportHelper.GetLowestCommonNamespace(appInstaller));

            // ForceUpdateFromAnyVersion should be supported from 1809 onward
            appInstaller = GetConfig(c => c.UpdateSettings = new UpdateSettings
            {
                ForceUpdateFromAnyVersion = true
            });
            Assert.AreEqual("http://schemas.microsoft.com/appx/appinstaller/2017/2", supportHelper.GetLowestCommonNamespace(appInstaller));
            appInstaller = GetConfig(c => c.UpdateSettings = new UpdateSettings
            {
                ForceUpdateFromAnyVersion = false
            });
            Assert.AreEqual("http://schemas.microsoft.com/appx/appinstaller/2017", supportHelper.GetLowestCommonNamespace(appInstaller));

            // UpdateBlocksActivation should be supported from 1903 onward
            appInstaller = GetConfig(c => c.UpdateSettings = new UpdateSettings
            {
                OnLaunch = new OnLaunchSettings
                {
                    UpdateBlocksActivation = true
                }
            });
            Assert.AreEqual("http://schemas.microsoft.com/appx/appinstaller/2018", supportHelper.GetLowestCommonNamespace(appInstaller));

            appInstaller = GetConfig(c => c.UpdateSettings = new UpdateSettings
            {
                OnLaunch = new OnLaunchSettings
                {
                    UpdateBlocksActivation = false
                }
            });
            Assert.AreEqual("http://schemas.microsoft.com/appx/appinstaller/2017", supportHelper.GetLowestCommonNamespace(appInstaller));

            // ShowPrompt should be supported from 1903 onward
            appInstaller = GetConfig(c => c.UpdateSettings = new UpdateSettings
            {
                OnLaunch = new OnLaunchSettings
                {
                    ShowPrompt = true
                }
            });
            Assert.AreEqual("http://schemas.microsoft.com/appx/appinstaller/2018", supportHelper.GetLowestCommonNamespace(appInstaller));

            appInstaller = GetConfig(c => c.UpdateSettings = new UpdateSettings
            {
                OnLaunch = new OnLaunchSettings
                {
                    ShowPrompt = false
                }
            });
            Assert.AreEqual("http://schemas.microsoft.com/appx/appinstaller/2017", supportHelper.GetLowestCommonNamespace(appInstaller));
        }

        private static AppInstallerConfig GetConfig(Action<AppInstallerConfig> configurator = null)
        {
            var newConfig = new AppInstallerConfig();
            configurator?.Invoke(newConfig);
            return newConfig;
        }
    }
}
