// MSIX Hero
// Copyright (C) 2024 Marcin Otorowski
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

using NUnit.Framework;
using Otor.MsixHero.AppInstaller;
using Otor.MsixHero.AppInstaller.Entities;

namespace Otor.MsixHero.Tests.Appx.AppInstaller
{
    [TestFixture]
    public class AppInstallerBuilderTests
    {
        [Test]
        public void TestUpdateSettings()
        {
            var builder = new AppInstallerBuilder();
            // ReSharper disable once JoinDeclarationAndInitializer
            AppInstallerConfig appInstaller;

            builder.CheckForUpdates = AppInstallerUpdateCheckingMethod.Never;
            appInstaller = builder.Build();
            Assert.That(appInstaller.UpdateSettings?.OnLaunch, Is.Null);
            Assert.That(appInstaller.UpdateSettings?.AutomaticBackgroundTask, Is.Null);

            builder.CheckForUpdates = AppInstallerUpdateCheckingMethod.Launch;
            appInstaller = builder.Build();
            Assert.That(appInstaller.UpdateSettings?.OnLaunch, Is.Not.Null);
            Assert.That(appInstaller.UpdateSettings?.AutomaticBackgroundTask, Is.Null);

            builder.CheckForUpdates = AppInstallerUpdateCheckingMethod.LaunchAndBackground;
            appInstaller = builder.Build();
            Assert.That(appInstaller.UpdateSettings?.OnLaunch, Is.Not.Null);
            Assert.That(appInstaller.UpdateSettings?.AutomaticBackgroundTask, Is.Not.Null);

            builder.CheckForUpdates = AppInstallerUpdateCheckingMethod.Background;
            appInstaller = builder.Build();
            Assert.That(appInstaller.UpdateSettings?.OnLaunch, Is.Null);
            Assert.That(appInstaller.UpdateSettings?.AutomaticBackgroundTask, Is.Not.Null);
        }

        [Test]
        public void TestVariousSettings()
        {
            var builder = new AppInstallerBuilder();
            // ReSharper disable once JoinDeclarationAndInitializer
            AppInstallerConfig appInstaller;
            
            builder.AllowDowngrades = true;
            appInstaller = builder.Build();
            Assert.That(appInstaller.UpdateSettings?.ForceUpdateFromAnyVersion, Is.True);
            
            builder.AllowDowngrades = false;
            appInstaller = builder.Build();
            Assert.That(appInstaller.UpdateSettings?.ForceUpdateFromAnyVersion, Is.Not.EqualTo(true));

            builder.CheckForUpdates = AppInstallerUpdateCheckingMethod.Launch;
            builder.UpdateBlocksActivation = true;
            appInstaller = builder.Build();
            Assert.That(appInstaller.UpdateSettings?.OnLaunch?.UpdateBlocksActivation, Is.True);

            builder.CheckForUpdates = AppInstallerUpdateCheckingMethod.Launch;
            builder.UpdateBlocksActivation = false;
            appInstaller = builder.Build();
            Assert.That(appInstaller.UpdateSettings?.OnLaunch?.UpdateBlocksActivation, Is.Not.EqualTo(true));

            builder.CheckForUpdates = AppInstallerUpdateCheckingMethod.Launch;
            builder.ShowPrompt = true;
            appInstaller = builder.Build();
            Assert.That(appInstaller.UpdateSettings?.OnLaunch?.ShowPrompt, Is.True);

            builder.CheckForUpdates = AppInstallerUpdateCheckingMethod.Launch;
            builder.ShowPrompt = false;
            appInstaller = builder.Build();
            Assert.That(appInstaller.UpdateSettings?.OnLaunch?.ShowPrompt, Is.Not.EqualTo(true));

            builder.HoursBetweenUpdateChecks = 20;
            appInstaller = builder.Build();
            Assert.That(appInstaller.UpdateSettings?.OnLaunch?.HoursBetweenUpdateChecks, Is.EqualTo(20));

            builder.HoursBetweenUpdateChecks = 24;
            appInstaller = builder.Build();
            Assert.That(appInstaller.UpdateSettings?.OnLaunch?.HoursBetweenUpdateChecks, Is.Not.EqualTo(24), "24 as a default value should be ignored.");
        }
    }
}
