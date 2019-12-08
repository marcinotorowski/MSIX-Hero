using System;
using System.IO;
using otor.msixhero.lib.Domain.Appx.AppInstaller;
using otor.msixhero.lib.Domain.Appx.Manifest.Summary;

namespace otor.msixhero.lib.BusinessLayer.Appx.AppInstaller
{
    public class AppInstallerBuilder
    {
        public AppInstallerUpdateCheckingMethod CheckForUpdates { get; set; }

        public bool AllowDowngrades { get; set; }

        public bool ShowPrompt { get; set; }
        
        public int HoursBetweenUpdateChecks { get; set; } = 24;

        public bool UpdateBlocksActivation { get; set; }

        public FileInfo MainPackageSource { get; set; }

        public Uri MainPackageUri { get; set; }

        public Uri RedirectUri { get; set; }

        public string MainPackageName { get; set; }
        
        public string MainPackagePublisher { get; set; }

        public string MainPackageVersion { get; set; }

        public PackageType MainPackageType { get; set; }

        public AppInstallerPackageArchitecture MainPackageArchitecture { get; set; }

        public AppInstallerConfig Build(PackageType packageType = PackageType.Package)
        {
            var appIns = new AppInstallerConfig();

            appIns.UpdateSettings = new UpdateSettings();
            switch (this.CheckForUpdates)
            {
                case AppInstallerUpdateCheckingMethod.Never:
                    appIns.UpdateSettings.OnLaunch = null;
                    appIns.UpdateSettings.AutomaticBackgroundTask = null;
                    break;
                case AppInstallerUpdateCheckingMethod.Launch:
                    appIns.UpdateSettings.OnLaunch = new OnLaunchSettings();
                    appIns.UpdateSettings.AutomaticBackgroundTask = null;
                    break;
                case AppInstallerUpdateCheckingMethod.LaunchAndBackground:
                    appIns.UpdateSettings.OnLaunch = new OnLaunchSettings();
                    appIns.UpdateSettings.AutomaticBackgroundTask = new AutomaticBackgroundTaskSettings();
                    break;
                case AppInstallerUpdateCheckingMethod.Background:
                    appIns.UpdateSettings.OnLaunch = null;
                    appIns.UpdateSettings.AutomaticBackgroundTask = new AutomaticBackgroundTaskSettings();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (this.AllowDowngrades)
            {
                appIns.UpdateSettings.ForceUpdateFromAnyVersion = true;
            }

            appIns.UpdateSettings.ForceUpdateFromAnyVersion = this.AllowDowngrades;

            if (appIns.UpdateSettings.OnLaunch != null)
            {
                appIns.UpdateSettings.OnLaunch.UpdateBlocksActivation = this.UpdateBlocksActivation;
                appIns.UpdateSettings.OnLaunch.ShowPrompt = this.ShowPrompt;

                if (this.HoursBetweenUpdateChecks != 24)
                {
                    appIns.UpdateSettings.OnLaunch.HoursBetweenUpdateChecks = this.HoursBetweenUpdateChecks;
                }
            }

            if (this.MainPackageSource != null)
            {
                var manifest = AppxManifestSummaryBuilder.FromMsix(this.MainPackageSource.FullName, AppxManifestSummaryBuilderMode.Identity).GetAwaiter().GetResult();

                if (packageType == PackageType.Bundle)
                {
                    appIns.MainBundle = new AppInstallerBundleEntry
                    {
                        Name = manifest.Name,
                        Version = manifest.Version,
                        Publisher = manifest.Publisher,
                        Uri = this.MainPackageUri?.ToString()
                    };
                }
                else
                {
                    appIns.MainPackage = new AppInstallerPackageEntry
                    {
                        Name = manifest.Name,
                        Version = manifest.Version,
                        Publisher = manifest.Publisher,
                        Uri = this.MainPackageUri?.ToString()
                    };

                    if (manifest.ProcessorArchitecture == null)
                    {
                        appIns.MainPackage.Architecture = AppInstallerPackageArchitecture.neutral;
                    }
                    else if (Enum.TryParse(manifest.ProcessorArchitecture, true, out AppInstallerPackageArchitecture parsed))
                    {
                        appIns.MainPackage.Architecture = parsed;
                    }
                }
            }
            else
            {
                if (packageType == PackageType.Bundle)
                {
                    appIns.MainBundle = new AppInstallerBundleEntry
                    {
                        Name = this.MainPackageName,
                        Version = this.MainPackageVersion,
                        Publisher = MainPackagePublisher,
                        Uri = this.MainPackageUri?.ToString()
                    };
                }
                else if (packageType == PackageType.Package)
                {
                    appIns.MainPackage = new AppInstallerPackageEntry
                    {
                        Name = this.MainPackageName,
                        Version = this.MainPackageVersion,
                        Publisher = MainPackagePublisher,
                        Uri = this.MainPackageUri?.ToString(),
                        Architecture = this.MainPackageArchitecture
                    };
                }
            }

            if (!appIns.UpdateSettings.ForceUpdateFromAnyVersion && appIns.UpdateSettings.OnLaunch == null && appIns.UpdateSettings.AutomaticBackgroundTask == null)
            {
                appIns.UpdateSettings = null;
            }

            if (this.RedirectUri != null)
            {
                appIns.Uri = this.RedirectUri.ToString();
            }

            return appIns;
        }
    }
}
