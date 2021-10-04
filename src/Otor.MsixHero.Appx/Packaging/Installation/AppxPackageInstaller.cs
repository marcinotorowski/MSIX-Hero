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

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Management.Deployment;
using Otor.MsixHero.Appx.Diagnostic.Developer;
using Otor.MsixHero.Appx.Diagnostic.Developer.Enums;
using Otor.MsixHero.Appx.Exceptions;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;
using Otor.MsixHero.Appx.Packaging.Interop;
using Otor.MsixHero.Appx.Packaging.Manifest;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities.Summary;
using Otor.MsixHero.Appx.Packaging.Manifest.FileReaders;
using Otor.MsixHero.Infrastructure.Helpers;
using Otor.MsixHero.Infrastructure.Logging;
using Otor.MsixHero.Infrastructure.Progress;

namespace Otor.MsixHero.Appx.Packaging.Installation
{
    [SuppressMessage("ReSharper", "UnusedVariable")]
    public class AppxPackageInstaller : IAppxPackageInstaller
    {
        private static readonly ILog Logger = LogManager.GetLogger();
        protected readonly ISideloadingChecker SideloadingChecker = new RegistrySideloadingChecker();
        
        public async Task Remove(IReadOnlyCollection<string> packages, bool preserveAppData = false, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            if (!packages.Any())
            {
                Logger.Warn("Removing 0 packages, the list from the user is empty.");
                return;
            }

            Logger.Info("Removing {0} packages...", packages.Count);

            var opts = RemovalOptions.None;
            if (preserveAppData)
            {
                opts |= RemovalOptions.PreserveApplicationData;
            }

            // ReSharper disable once PossibleMultipleEnumeration
            foreach (var item in packages)
            {
                Logger.Info("Removing {0}", item);

                var task = AsyncOperationHelper.ConvertToTask(
                    PackageManagerWrapper.Instance.RemovePackageAsync(item, opts),
                    "Removing...", CancellationToken.None, progress);

                await task.ConfigureAwait(false);
            }
        }

        public async Task Remove(IReadOnlyCollection<InstalledPackage> packages, bool forAllUsers = false, bool preserveAppData = false, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            if (!packages.Any())
            {
                Logger.Warn("Removing 0 packages, the list from the user is empty.");
                return;
            }

            Logger.Info("Removing {0} packages...", packages.Count);

            var opts = RemovalOptions.None;
            if (preserveAppData)
            {
                opts |= RemovalOptions.PreserveApplicationData;
            }

            if (forAllUsers)
            {
                opts |= RemovalOptions.RemoveForAllUsers;
            }

            // ReSharper disable once PossibleMultipleEnumeration
            foreach (var item in packages)
            {
                Logger.Info("Removing {0}", item.PackageId);

                var task = AsyncOperationHelper.ConvertToTask(
                    PackageManagerWrapper.Instance.RemovePackageAsync(item.PackageId, opts),
                    $"Removing {item.DisplayName}...", CancellationToken.None, progress);

                await task.ConfigureAwait(false);

                if (item.IsProvisioned && forAllUsers)
                {
                    await Deprovision(item.PackageFamilyName, cancellationToken, progress).ConfigureAwait(false);
                }
            }
        }
        
        public async Task Deprovision(string packageFamilyName, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var task = AsyncOperationHelper.ConvertToTask(
                PackageManagerWrapper.Instance.DeprovisionPackageForAllUsersAsync(packageFamilyName),
                "De-provisioning for all users",
                CancellationToken.None, progress);
            await task.ConfigureAwait(false);
        }

        public async Task Add(string filePath, AddAppxPackageOptions options = 0, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            if (SideloadingChecker.GetStatus() < SideloadingStatus.Sideloading)
            {
                throw new DeveloperModeException("Developer mode or sideloading must be enabled to install packages outside of Microsoft Store.");
            }

            Logger.Info("Installing package {0}", filePath);
            if (filePath == null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            if (string.Equals(Path.GetFileName(filePath), FileConstants.AppxManifestFile, StringComparison.OrdinalIgnoreCase))
            {
                if (options.HasFlag(AddAppxPackageOptions.AllBundleResources))
                {
                    throw new ArgumentException("Cannot use the flag AllBundleResources with non-bundle packages.", nameof(options));
                }

                var reader = await AppxManifestSummaryReader.FromManifest(filePath).ConfigureAwait(false);

                DeploymentOptions deploymentOptions = 0;

                if (options.HasFlag(AddAppxPackageOptions.AllowDowngrade))
                {
                    deploymentOptions |= DeploymentOptions.ForceUpdateFromAnyVersion;
                }

                if (options.HasFlag(AddAppxPackageOptions.KillRunningApps))
                {
                    deploymentOptions |= DeploymentOptions.ForceApplicationShutdown;
                    deploymentOptions |= DeploymentOptions.ForceTargetApplicationShutdown;
                }

                deploymentOptions |= DeploymentOptions.DevelopmentMode;

                await AsyncOperationHelper.ConvertToTask(
                    PackageManagerWrapper.Instance.RegisterPackageAsync(new Uri(filePath), Enumerable.Empty<Uri>(), deploymentOptions),
                    $"Installing {reader.DisplayName} {reader.Version}...",
                    cancellationToken,
                    progress).ConfigureAwait(false);
            }
            else if (string.Equals(FileConstants.AppInstallerExtension, Path.GetExtension(filePath), StringComparison.OrdinalIgnoreCase))
            {
                if (options.HasFlag(AddAppxPackageOptions.AllUsers))
                {
                    throw new ArgumentException("Cannot install a package from .appinstaller for all users.", nameof(options));
                }

                if (options.HasFlag(AddAppxPackageOptions.AllBundleResources))
                {
                    throw new ArgumentException("Cannot use the flag AllBundleResources with non-bundle packages.", nameof(options));
                }

                if (options.HasFlag(AddAppxPackageOptions.AllowDowngrade))
                {
                    throw new ArgumentException("Cannot force a downgrade with .appinstaller. The .appinstaller defines on its own whether the downgrade is allowed.", nameof(options));
                }

                AddPackageByAppInstallerOptions deploymentOptions = 0;

                if (options.HasFlag(AddAppxPackageOptions.KillRunningApps))
                {
                    deploymentOptions |= AddPackageByAppInstallerOptions.ForceTargetAppShutdown;
                }

                var volume = PackageManagerWrapper.Instance.GetDefaultPackageVolume();
                await AsyncOperationHelper.ConvertToTask(
                    PackageManagerWrapper.Instance.AddPackageByAppInstallerFileAsync(new Uri(filePath, UriKind.Absolute), deploymentOptions, volume),
                    "Installing from " + Path.GetFileName(filePath) + "...",
                    cancellationToken,
                    progress).ConfigureAwait(false);
            }
            else
            {
                string name, version, publisher;

                DeploymentOptions deploymentOptions = 0;

                switch (Path.GetExtension(filePath))
                {
                    case FileConstants.AppxBundleExtension:
                    case FileConstants.MsixBundleExtension:
                    {
                        IAppxIdentityReader reader = new AppxIdentityReader();
                        var identity = await reader.GetIdentity(filePath, cancellationToken).ConfigureAwait(false);
                        name = identity.Name;
                        publisher = identity.Publisher;
                        version = identity.Version;

                        if (options.HasFlag(AddAppxPackageOptions.AllBundleResources))
                        {
                            deploymentOptions |= DeploymentOptions.InstallAllResources;
                        }

                        break;
                    }

                    default:
                    {
                        if (options.HasFlag(AddAppxPackageOptions.AllBundleResources))
                        {
                            throw new ArgumentException("Cannot use the flag AllBundleResources with non-bundle packages.", nameof(options));
                        }

                        var reader = await AppxManifestSummaryReader.FromMsix(filePath).ConfigureAwait(false);
                        name = reader.DisplayName;
                        version = reader.Version;
                        publisher = reader.Publisher;
                        break;
                    }
                }

                if (options.HasFlag(AddAppxPackageOptions.AllowDowngrade))
                {
                    deploymentOptions |= DeploymentOptions.ForceUpdateFromAnyVersion;
                }

                if (options.HasFlag(AddAppxPackageOptions.KillRunningApps))
                {
                    deploymentOptions |= DeploymentOptions.ForceApplicationShutdown;
                    deploymentOptions |= DeploymentOptions.ForceTargetApplicationShutdown;
                }

                if (options.HasFlag(AddAppxPackageOptions.AllUsers))
                {
                    var deploymentResult = await AsyncOperationHelper.ConvertToTask(
                        PackageManagerWrapper.Instance.AddPackageAsync(new Uri(filePath, UriKind.Absolute), Enumerable.Empty<Uri>(), deploymentOptions),
                        $"Installing {name} {version}...",
                        cancellationToken,
                        progress).ConfigureAwait(false);

                    if (!deploymentResult.IsRegistered)
                    {
                        throw new InvalidOperationException("The package could not be registered.");
                    }

                    var findInstalled = PackageManagerWrapper.Instance.FindPackages(name, publisher).FirstOrDefault();
                    if (findInstalled == null)
                    {
                        throw new InvalidOperationException("The package could not be registered.");
                    }

                    var familyName = findInstalled.Id.FamilyName;

                    await AsyncOperationHelper.ConvertToTask(
                        PackageManagerWrapper.Instance.ProvisionPackageForAllUsersAsync(familyName),
                        $"Provisioning {name} {version}...",
                        cancellationToken,
                        progress).ConfigureAwait(false);
                }
                else
                {
                    var deploymentResult = await AsyncOperationHelper.ConvertToTask(
                        PackageManagerWrapper.Instance.AddPackageAsync(new Uri(filePath, UriKind.Absolute), Enumerable.Empty<Uri>(), deploymentOptions),
                        "Installing " + name + "...",
                        cancellationToken,
                        progress).ConfigureAwait(false);

                    if (!deploymentResult.IsRegistered)
                    {
                        var message = "Could not install " + name + " " + version + ".";
                        if (!string.IsNullOrEmpty(deploymentResult.ErrorText))
                        {
                            message += " " + deploymentResult.ErrorText;
                        }

                        if (deploymentResult.ExtendedErrorCode != null)
                        {
                            throw new InvalidOperationException(message, deploymentResult.ExtendedErrorCode);
                        }

                        throw new InvalidOperationException(message);
                    }
                }
            }
        }
        
        public async Task<bool> IsInstalled(string manifestPath, PackageFindMode mode = PackageFindMode.CurrentUser, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            PackageFindMode actualMode = mode;
            if (actualMode == PackageFindMode.Auto)
            {
                var isAdmin = await UserHelper.IsAdministratorAsync(cancellationToken).ConfigureAwait(false);
                if (isAdmin)
                {
                    actualMode = PackageFindMode.AllUsers;
                }
                else
                {
                    actualMode = PackageFindMode.CurrentUser;
                }
            }

            string pkgFullName;

            using (var src = FileReaderFactory.CreateFileReader(manifestPath))
            {
                var manifestReader = new AppxManifestReader();
                var parsed = await manifestReader.Read(src, false, cancellationToken).ConfigureAwait(false);
                pkgFullName = parsed.FullName;
            }

            switch (actualMode)
            {
                case PackageFindMode.CurrentUser:
                    var pkg = await Task.Run(
                        () => PackageManagerWrapper.Instance.FindPackageForUser(string.Empty, pkgFullName),
                        cancellationToken).ConfigureAwait(false);
                    return pkg != null;
                case PackageFindMode.AllUsers:
                    var pkgAllUsers = await Task.Run(
                        () => PackageManagerWrapper.Instance.FindPackage(pkgFullName),
                        cancellationToken).ConfigureAwait(false);
                    return pkgAllUsers != null;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}