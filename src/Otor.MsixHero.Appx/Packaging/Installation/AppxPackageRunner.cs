﻿// MSIX Hero
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

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Diagnostic.Developer;
using Otor.MsixHero.Appx.Diagnostic.Developer.Enums;
using Otor.MsixHero.Appx.Exceptions;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;
using Otor.MsixHero.Appx.Packaging.Manifest;
using Otor.MsixHero.Appx.Packaging.Manifest.FileReaders;
using Otor.MsixHero.Infrastructure.Logging;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.ThirdParty.PowerShell;

namespace Otor.MsixHero.Appx.Packaging.Installation
{
    [SuppressMessage("ReSharper", "UnusedVariable")]
    public class AppxPackageRunner : IAppxPackageRunner
    {
        private static readonly ILog Logger = LogManager.GetLogger();
        protected readonly ISideloadingChecker SideloadingChecker = new RegistrySideloadingChecker();
        
        public async Task RunToolInContext(InstalledPackage package, string toolPath, string arguments, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            if (package == null)
            {
                throw new ArgumentNullException(nameof(package));
            }

            if (toolPath == null)
            {
                throw new ArgumentNullException(nameof(toolPath));
            }

            using (IAppxFileReader reader = new FileInfoFileReaderAdapter(package.ManifestLocation))
            {
                var maniReader = new AppxManifestReader();
                var manifest = await maniReader.Read(reader, cancellationToken).ConfigureAwait(false);

                if (!manifest.Applications.Any())
                {
                    throw new InvalidOperationException("Cannot execute a command in this package context. The package does not have any applications defined.");
                }

                await RunToolInContext(package.PackageFamilyName, manifest.Applications[0].Id, toolPath, arguments, cancellationToken, progress).ConfigureAwait(false);
            }
        }

        public async Task RunToolInContext(string packageFamilyName, string appId, string toolPath, string arguments = null, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            if (SideloadingChecker.GetStatus() != SideloadingStatus.DeveloperMode)
            {
                throw new DeveloperModeException("Developer mode must be enabled to use this feature.");
            }

            Logger.Info("Running tool '{0}' with arguments '{1}' in package '{2}' (AppId = '{3}')...", toolPath, arguments, packageFamilyName, appId);
            if (packageFamilyName == null)
            {
                throw new ArgumentNullException(nameof(packageFamilyName));
            }

            if (toolPath == null)
            {
                throw new ArgumentNullException(nameof(toolPath));
            }

            using var ps = await PowerShellSession.CreateForAppxModule().ConfigureAwait(false);
            using var cmd = ps.AddCommand("Invoke-CommandInDesktopPackage");
            cmd.AddParameter("Command", toolPath);
            cmd.AddParameter("PackageFamilyName", packageFamilyName);
            cmd.AddParameter("AppId", appId);
            cmd.AddParameter("PreventBreakaway");

            if (!string.IsNullOrEmpty(arguments))
            {
                cmd.AddParameter("Args", arguments);
            }

            Logger.Debug("Executing Invoke-CommandInDesktopPackage");
            try
            {
                // ReSharper disable once UnusedVariable
                using var result = await ps.InvokeAsync().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                if (e.HResult == -2147024891 /* 0x80070005 E_ACCESSDENIED */)
                {
                    throw new DeveloperModeException("Developer mode must be enabled to use this feature.", e);
                }

                if (e.HResult == -2146233087)
                {
                    throw new AdminRightsRequiredException("This tool requires admin rights.", e);
                }

                throw;
            }
        }

        public Task Run(InstalledPackage package, string appId = null, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            return Run(package.ManifestLocation, appId, cancellationToken, progress);
        }
        
        public async Task Run(string packageManifestLocation, string appId = null, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            if (appId == null)
            {
                Logger.Info("Running the default entry point from package " + packageManifestLocation);
            }

            if (packageManifestLocation == null || !File.Exists(packageManifestLocation))
            {
                throw new FileNotFoundException();
            }

            string entryPoint;
            if (string.IsNullOrEmpty(appId))
            {
                entryPoint = (await GetEntryPoints(packageManifestLocation).ConfigureAwait(false)).FirstOrDefault();
            }
            else
            {
                entryPoint = (await GetEntryPoints(packageManifestLocation).ConfigureAwait(false)).FirstOrDefault(e => e.EndsWith("!" + appId, StringComparison.Ordinal));
            }

            if (entryPoint == null)
            {
                if (appId == null)
                {
                    Logger.Warn("The package does not contain any entry point that is visible in the start menu. Aborting...");
                    throw new InvalidOperationException("This package has no Start Menu entry points.");
                }

                throw new InvalidOperationException($"The entry point '{appId}' was not found.");
            }

            var p = new Process();
            var startInfo = new ProcessStartInfo
            {
                UseShellExecute = true,
                FileName = entryPoint
            };

            Logger.Info("Executing " + entryPoint + " (with shell execute)...");
            p.StartInfo = startInfo;
            p.Start();
        }
        
        private static async Task<string[]> GetEntryPoints(string manifestLocation)
        {
            if (!File.Exists(manifestLocation))
            {
                return new string[0];
            }

            var reader = new AppxManifestReader();
            using (IAppxFileReader appxSource = new FileInfoFileReaderAdapter(manifestLocation))
            {
                var appxPackage = await reader.Read(appxSource).ConfigureAwait(false);

                return appxPackage.Applications.Select(app =>
                {
                    if (string.IsNullOrEmpty(app.Id))
                    {
                        return @"shell:appsFolder\" + appxPackage.FamilyName;
                    }

                    return @"shell:appsFolder\" + appxPackage.FamilyName + "!" + app.Id;
                }).ToArray();
            }
        }
    }
}