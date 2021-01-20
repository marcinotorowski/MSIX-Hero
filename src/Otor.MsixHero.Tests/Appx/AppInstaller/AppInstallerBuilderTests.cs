using System;
using System.Collections.Generic;
using System.Text;
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
            Assert.IsNull(appInstaller.UpdateSettings?.OnLaunch);
            Assert.IsNull(appInstaller.UpdateSettings?.AutomaticBackgroundTask);

            builder.CheckForUpdates = AppInstallerUpdateCheckingMethod.Launch;
            appInstaller = builder.Build();
            Assert.IsNotNull(appInstaller.UpdateSettings?.OnLaunch);
            Assert.IsNull(appInstaller.UpdateSettings?.AutomaticBackgroundTask);

            builder.CheckForUpdates = AppInstallerUpdateCheckingMethod.LaunchAndBackground;
            appInstaller = builder.Build();
            Assert.IsNotNull(appInstaller.UpdateSettings?.OnLaunch);
            Assert.IsNotNull(appInstaller.UpdateSettings?.AutomaticBackgroundTask);

            builder.CheckForUpdates = AppInstallerUpdateCheckingMethod.Background;
            appInstaller = builder.Build();
            Assert.IsNull(appInstaller.UpdateSettings?.OnLaunch);
            Assert.IsNotNull(appInstaller.UpdateSettings?.AutomaticBackgroundTask);
        }

        [Test]
        public void TestVariousSettings()
        {
            var builder = new AppInstallerBuilder();
            // ReSharper disable once JoinDeclarationAndInitializer
            AppInstallerConfig appInstaller;
            
            builder.AllowDowngrades = true;
            appInstaller = builder.Build();
            Assert.IsTrue(appInstaller.UpdateSettings?.ForceUpdateFromAnyVersion);
            
            builder.AllowDowngrades = false;
            appInstaller = builder.Build();
            Assert.IsTrue(appInstaller.UpdateSettings?.ForceUpdateFromAnyVersion != true);

            builder.CheckForUpdates = AppInstallerUpdateCheckingMethod.Launch;
            builder.UpdateBlocksActivation = true;
            appInstaller = builder.Build();
            Assert.IsTrue(appInstaller.UpdateSettings?.OnLaunch?.UpdateBlocksActivation);

            builder.CheckForUpdates = AppInstallerUpdateCheckingMethod.Launch;
            builder.UpdateBlocksActivation = false;
            appInstaller = builder.Build();
            Assert.IsTrue(appInstaller.UpdateSettings?.OnLaunch?.UpdateBlocksActivation != true);

            builder.CheckForUpdates = AppInstallerUpdateCheckingMethod.Launch;
            builder.ShowPrompt = true;
            appInstaller = builder.Build();
            Assert.IsTrue(appInstaller.UpdateSettings?.OnLaunch?.ShowPrompt);

            builder.CheckForUpdates = AppInstallerUpdateCheckingMethod.Launch;
            builder.ShowPrompt = false;
            appInstaller = builder.Build();
            Assert.IsTrue(appInstaller.UpdateSettings?.OnLaunch?.ShowPrompt != true);

            builder.HoursBetweenUpdateChecks = 20;
            appInstaller = builder.Build();
            Assert.AreEqual(20, appInstaller.UpdateSettings?.OnLaunch?.HoursBetweenUpdateChecks);

            builder.HoursBetweenUpdateChecks = 24;
            appInstaller = builder.Build();
            Assert.AreNotEqual(24, appInstaller.UpdateSettings?.OnLaunch?.HoursBetweenUpdateChecks, "24 as a default value should be ignored.");
        }
    }
}
