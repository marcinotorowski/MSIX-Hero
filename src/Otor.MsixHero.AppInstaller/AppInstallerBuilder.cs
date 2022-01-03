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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Otor.MsixHero.AppInstaller.Entities;
using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities.Summary;
using Otor.MsixHero.Appx.Packaging.Manifest.Enums;
using Otor.MsixHero.Infrastructure.Progress;

namespace Otor.MsixHero.AppInstaller
{
    public class AppInstallerBuilder
    {
        public AppInstallerBuilder()
        {
        }

        public AppInstallerBuilder(AppInstallerConfig appInstaller)
        {
            this.ShowPrompt = appInstaller.UpdateSettings?.OnLaunch?.ShowPrompt == true;
            this.HoursBetweenUpdateChecks = appInstaller.UpdateSettings?.OnLaunch?.HoursBetweenUpdateChecks ?? 0;
            this.Version = appInstaller.Version;
            this.AllowDowngrades = appInstaller.UpdateSettings?.ForceUpdateFromAnyVersion == true;
            this.UpdateBlocksActivation = appInstaller.UpdateSettings?.OnLaunch?.UpdateBlocksActivation == true;
            this.RedirectUri = Uri.TryCreate(appInstaller.Uri ?? string.Empty, UriKind.Absolute, out var parsed) ? parsed  : null;

            var checkOnLaunch = appInstaller.UpdateSettings?.OnLaunch != null;
            var checkBackground = appInstaller.UpdateSettings?.AutomaticBackgroundTask != null;

            if (checkBackground && checkOnLaunch)
            {
                this.CheckForUpdates = AppInstallerUpdateCheckingMethod.LaunchAndBackground;
            }
            else if (checkBackground)
            {
                this.CheckForUpdates = AppInstallerUpdateCheckingMethod.Background;
            }
            else if (checkOnLaunch)
            {
                this.CheckForUpdates = AppInstallerUpdateCheckingMethod.Launch;
            }
            else
            {
                this.CheckForUpdates = AppInstallerUpdateCheckingMethod.Never;
            }
            
            this.MainPackageUri = Uri.TryCreate(appInstaller.MainPackage?.Uri ?? appInstaller.MainBundle?.Uri ?? string.Empty, UriKind.Absolute, out var parsedUri) ? parsedUri : null;

            if (appInstaller.MainPackage?.Architecture != null)
            {
                this.MainPackageArchitecture = (AppxPackageArchitecture)Enum.Parse(typeof(AppxPackageArchitecture), appInstaller.MainPackage.Architecture.ToString("G"), true);
            }
            else
            {
                this.MainPackageArchitecture = AppxPackageArchitecture.Neutral;
            }

            this.MainPackageVersion = appInstaller.MainPackage?.Version ?? appInstaller.MainPackage?.Version;
            this.MainPackageType = appInstaller.MainPackage == null && appInstaller.MainBundle != null ? PackageType.Bundle : PackageType.Package;
            this.MainPackagePublisher = appInstaller.MainPackage?.Publisher ?? appInstaller.MainBundle?.Publisher;
            this.MainPackageName = appInstaller.MainPackage?.Name ?? appInstaller.MainBundle?.Name;
        } 

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

        public string Version { get; set; }

        public PackageType MainPackageType { get; set; }

        public async Task Create(AppInstallerConfig config, string file, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var fileInfo = new FileInfo(file);
            // ReSharper disable once PossibleNullReferenceException
            if (!fileInfo.Directory.Exists)
            {
                fileInfo.Directory.Create();
            }

            using (var textWriter = new Utf8StringWriter())
            {
                var supportHelper = new AppInstallerFeatureSupportHelper();
                var lowestNamespace = supportHelper.GetLowestCommonNamespace(config);
                
                var xmlns = new XmlSerializerNamespaces();
                xmlns.Add("", lowestNamespace);

                switch (lowestNamespace)
                {
                    case "http://schemas.microsoft.com/appx/appinstaller/2017":
                        new XmlSerializer(typeof(AppInstallerConfig2017)).Serialize(textWriter, new AppInstallerConfig2017(config), xmlns);
                        break;
                    case "http://schemas.microsoft.com/appx/appinstaller/2017/2":
                        new XmlSerializer(typeof(AppInstallerConfig20172)).Serialize(textWriter, new AppInstallerConfig20172(config), xmlns);
                        break;
                    case "http://schemas.microsoft.com/appx/appinstaller/2018":
                        new XmlSerializer(typeof(AppInstallerConfig2018)).Serialize(textWriter, new AppInstallerConfig2018(config), xmlns);
                        break;
                    default:
                        throw new NotSupportedException("This feature set is not supported.");
                }

                if (File.Exists(file))
                {
                    File.Delete(file);
                }

                var enc = new UTF8Encoding(false, false);
                await File.WriteAllTextAsync(file, textWriter.ToString(), enc, cancellationToken).ConfigureAwait(false);
            }
        }
        
        public AppxPackageArchitecture MainPackageArchitecture { get; set; }

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
                AppxIdentity identity;
                var identityReader = new AppxIdentityReader();

                try
                {
                    identity = identityReader.GetIdentity(this.MainPackageSource.FullName).Result;
                }
                catch (AggregateException e)
                {
                    throw e.GetBaseException();
                }
                
                if (packageType == PackageType.Bundle)
                {
                    appIns.MainBundle = new AppInstallerBundleEntry
                    {
                        Name = identity.Name,
                        Version = identity.Version,
                        Publisher = identity.Publisher,
                        Uri = this.MainPackageUri?.ToString()
                    };
                }
                else
                {
                    appIns.MainPackage = new AppInstallerPackageEntry
                    {
                        Name = identity.Name,
                        Version = identity.Version,
                        Publisher = identity.Publisher,
                        Uri = this.MainPackageUri?.ToString()
                    };

                    if (identity.Architectures?.Any() != true)
                    {
                        appIns.MainPackage.Architecture = AppInstallerPackageArchitecture.neutral;
                    }
                    else 
                    {
                        var arch = identity.Architectures.First().ToString("G");
                        if (Enum.TryParse(arch, true, out AppInstallerPackageArchitecture parsed))
                        {
                            appIns.MainPackage.Architecture = parsed;
                        }
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
                        Architecture = (AppInstallerPackageArchitecture)Enum.Parse(typeof(AppInstallerPackageArchitecture), this.MainPackageArchitecture.ToString("G"), true)
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

            appIns.Version = this.Version ?? "1.0.0.0";

            return appIns;
        }

        private sealed class Utf8StringWriter : StringWriter
        {
            public override Encoding Encoding => Encoding.UTF8;
        }
    }
}
