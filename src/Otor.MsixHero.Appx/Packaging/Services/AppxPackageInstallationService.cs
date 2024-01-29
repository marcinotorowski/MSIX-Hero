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

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.Management.Deployment;
using Otor.MsixHero.Appx.Diagnostic.Developer;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;
using Otor.MsixHero.Appx.Packaging.Interop;
using Otor.MsixHero.Appx.Packaging.Manifest;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities.Summary;
using Otor.MsixHero.Appx.Packaging.Manifest.FileReaders;
using Otor.MsixHero.Infrastructure.Helpers;
using Dapplo.Log;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Appx.Packaging.Installation;

namespace Otor.MsixHero.Appx.Packaging.Services
{
    [SuppressMessage("ReSharper", "UnusedVariable")]
    public class AppxPackageInstallationService : IAppxPackageInstallationService
    {
        private static readonly LogSource Logger = new(); protected readonly ISideloadingConfigurator SideloadingConfigurator = new SideloadingConfigurator();

        public async Task Remove(IReadOnlyCollection<string> packages, bool preserveAppData = false, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            if (!packages.Any())
            {
                Logger.Warn().WriteLine("Removing 0 packages, the list from the user is empty.");
                return;
            }

            Logger.Info().WriteLine("Removing {0} packages…", packages.Count);

            var opts = RemovalOptions.None;
            if (preserveAppData)
            {
                opts |= RemovalOptions.PreserveApplicationData;
            }

            // ReSharper disable once PossibleMultipleEnumeration
            foreach (var item in packages)
            {
                Logger.Info().WriteLine("Removing {0}", item);

                var task = AsyncOperationHelper.ConvertToTask(
                    PackageManagerSingleton.Instance.RemovePackageAsync(item, opts),
                    Resources.Localization.Packages_Removing, CancellationToken.None, progress);

                await task.ConfigureAwait(false);
            }
        }

        public async Task Remove(IReadOnlyCollection<PackageEntry> packages, bool forAllUsers = false, bool preserveAppData = false, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            if (!packages.Any())
            {
                Logger.Warn().WriteLine("Removing 0 packages, the list from the user is empty.");
                return;
            }

            Logger.Info().WriteLine("Removing {0} packages…", packages.Count);

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
                Logger.Info().WriteLine("Removing {0}", item.PackageFullName);

                var task = AsyncOperationHelper.ConvertToTask(
                    PackageManagerSingleton.Instance.RemovePackageAsync(item.PackageFullName, opts),
                    string.Format(Resources.Localization.Packages_Removing_Format, item.DisplayName), CancellationToken.None, progress);

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
                PackageManagerSingleton.Instance.DeprovisionPackageForAllUsersAsync(packageFamilyName),
                Resources.Localization.Packages_Deprovision_AllUsers,
                CancellationToken.None, progress);
            await task.ConfigureAwait(false);
        }

        public async Task Add(string filePath, AddAppxPackageOptions options = 0, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            try
            {
                SideloadingConfigurator.AssertSideloadingEnabled();

                Logger.Info().WriteLine("Installing package {0}", filePath);
                if (filePath == null)
                {
                    throw new ArgumentNullException(nameof(filePath));
                }

                if (string.Equals(Path.GetFileName(filePath), FileConstants.AppxManifestFile, StringComparison.OrdinalIgnoreCase))
                {
                    if (options.HasFlag(AddAppxPackageOptions.AllBundleResources))
                    {
                        throw new ArgumentException(Resources.Localization.Packages_Error_AllBundleResources, nameof(options));
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
                        PackageManagerSingleton.Instance.RegisterPackageAsync(new Uri(filePath), Enumerable.Empty<Uri>(), deploymentOptions),
                        $"Installing {reader.DisplayName} {reader.Version}…",
                        cancellationToken,
                        progress).ConfigureAwait(false);
                }
                else if (string.Equals(FileConstants.AppInstallerExtension, Path.GetExtension(filePath), StringComparison.OrdinalIgnoreCase))
                {
                    if (options.HasFlag(AddAppxPackageOptions.AllUsers))
                    {
                        throw new ArgumentException(Resources.Localization.Packages_Error_AppInstallerAllUsers, nameof(options));
                    }

                    if (options.HasFlag(AddAppxPackageOptions.AllBundleResources))
                    {
                        throw new ArgumentException(Resources.Localization.Packages_Error_AllBundleResources, nameof(options));
                    }

                    if (options.HasFlag(AddAppxPackageOptions.AllowDowngrade))
                    {
                        throw new ArgumentException(Resources.Localization.Packages_Error_AppInstallerDowngrade, nameof(options));
                    }

                    AddPackageByAppInstallerOptions deploymentOptions = 0;

                    if (options.HasFlag(AddAppxPackageOptions.KillRunningApps))
                    {
                        deploymentOptions |= AddPackageByAppInstallerOptions.ForceTargetAppShutdown;
                    }

                    var volume = PackageManagerSingleton.Instance.GetDefaultPackageVolume();
                    await AsyncOperationHelper.ConvertToTask(
                        PackageManagerSingleton.Instance.AddPackageByAppInstallerFileAsync(new Uri(filePath, UriKind.Absolute), deploymentOptions, volume),
                        string.Format(Resources.Localization.Packages_InstallingFrom_Format, Path.GetFileName(filePath)),
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
                                    throw new ArgumentException(Resources.Localization.Packages_Error_AllBundleResources, nameof(options));
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
                            PackageManagerSingleton.Instance.AddPackageAsync(new Uri(filePath, UriKind.Absolute), Enumerable.Empty<Uri>(), deploymentOptions),
                            $"Installing {name} {version}…",
                            cancellationToken,
                            progress).ConfigureAwait(false);

                        if (!deploymentResult.IsRegistered)
                        {
                            throw new InvalidOperationException(Resources.Localization.Packages_Error_Registering);
                        }

                        var findInstalled = PackageManagerSingleton.Instance.FindPackages(name, publisher).FirstOrDefault();
                        if (findInstalled == null)
                        {
                            throw new InvalidOperationException(Resources.Localization.Packages_Error_Registering);
                        }

                        var familyName = findInstalled.Id.FamilyName;

                        await AsyncOperationHelper.ConvertToTask(
                            PackageManagerSingleton.Instance.ProvisionPackageForAllUsersAsync(familyName),
                            string.Format(Resources.Localization.Packages_Provisioning_Format, name, version),
                            cancellationToken,
                            progress).ConfigureAwait(false);
                    }
                    else
                    {
                        var deploymentResult = await AsyncOperationHelper.ConvertToTask(
                            PackageManagerSingleton.Instance.AddPackageAsync(new Uri(filePath, UriKind.Absolute), Enumerable.Empty<Uri>(), deploymentOptions),
                            string.Format(Resources.Localization.Packages_Installing_Format, name),
                            cancellationToken,
                            progress).ConfigureAwait(false);

                        if (!deploymentResult.IsRegistered)
                        {
                            var message = string.Format(Resources.Localization.Packages_Error_Install_Format, name, version);
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
            catch (COMException e)
            {
                if (e.ErrorCode == -2146762487)
                {
                    throw new InvalidOperationException("", e); // todo: revisit this!
                }

                throw;
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
                        () => PackageManagerSingleton.Instance.FindPackageForUser(string.Empty, pkgFullName),
                        cancellationToken).ConfigureAwait(false);
                    return pkg != null;
                case PackageFindMode.AllUsers:
                    var pkgAllUsers = await Task.Run(
                        () => PackageManagerSingleton.Instance.FindPackage(pkgFullName),
                        cancellationToken).ConfigureAwait(false);
                    return pkgAllUsers != null;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}