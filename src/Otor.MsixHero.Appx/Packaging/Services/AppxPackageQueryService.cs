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
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Management.Deployment;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;
using Otor.MsixHero.Appx.Users;
using Otor.MsixHero.Infrastructure.Helpers;
using Dapplo.Log;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Appx.Common.Enums;
using Otor.MsixHero.Appx.Reader.File;
using Otor.MsixHero.Appx.Reader.Manifest;
using Otor.MsixHero.Appx.Reader.Manifest.Entities;
using Otor.MsixHero.Appx.Common.Identity;
using Otor.MsixHero.Appx.Reader.File.Adapters;

namespace Otor.MsixHero.Appx.Packaging.Services;

public class AppxPackageQueryService(IConfigurationService configurationService) : IAppxPackageQueryService
{
    private static readonly LogSource Logger = new();

    public Task<List<User>> GetUsersForPackage(PackageEntry packageEntry, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
    {
        return GetUsersForPackage(packageEntry.Name, cancellationToken, progress);
    }

    public async Task<List<User>> GetUsersForPackage(string packageName, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
    {
        Logger.Info().WriteLine("Getting users who installed package {0}…", packageName);
        if (!await UserHelper.IsAdministratorAsync(cancellationToken).ConfigureAwait(false))
        {
            Logger.Info().WriteLine("The user is not administrator. Returning an empty list.");
            return new List<User>();
        }

        var result = await Task.Run(
            () =>
            {
                var list = PackageManagerSingleton.Instance.FindUsers(packageName).Select(u => new User(SidToAccountName(u.UserSecurityId))).ToList();
                return list;
            },
            cancellationToken).ConfigureAwait(false);

        Logger.Info().WriteLine("Returning {0} users…", result.Count);
        return result;
    }

    public Task<List<PackageEntry>> GetInstalledPackages(PackageQuerySourceType mode = PackageQuerySourceType.Installed, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
    {
        switch (mode)
        {
            case PackageQuerySourceType.Installed:
            case PackageQuerySourceType.InstalledForCurrentUser:
            case PackageQuerySourceType.InstalledForAllUsers:
                break;
            default:
                throw new NotSupportedException($"Mode {mode} is not supported by this method.");
        }

        return this.QueryInstalledPackages(null, mode, cancellationToken, progress);
    }

    public async Task<List<PackageEntry>> GetModificationPackages(string packageFullName, PackageQuerySourceType mode = PackageQuerySourceType.Installed, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
    {
        switch (mode)
        {
            case PackageQuerySourceType.Installed:
            case PackageQuerySourceType.InstalledForCurrentUser:
            case PackageQuerySourceType.InstalledForAllUsers:
                break;
            default:
                throw new NotSupportedException($"Mode {mode} is not supported by this method.");
        }

        if (mode == PackageQuerySourceType.Installed)
        {
            mode = await UserHelper.IsAdministratorAsync(cancellationToken).ConfigureAwait(false) ? PackageQuerySourceType.InstalledForAllUsers : PackageQuerySourceType.InstalledForCurrentUser;
        }

        var find = await Task.Run(() => mode == PackageQuerySourceType.InstalledForCurrentUser ? PackageManagerSingleton.Instance.FindPackageForUser(string.Empty, packageFullName) : PackageManagerSingleton.Instance.FindPackage(packageFullName), cancellationToken).ConfigureAwait(false);
        if (find == null)
        {
            var packageIdentity = PackageIdentity.FromFullName(packageFullName);
            find = await Task.Run(() => mode == PackageQuerySourceType.InstalledForCurrentUser ? PackageManagerSingleton.Instance.FindPackageForUser(string.Empty, packageIdentity.AppName) : PackageManagerSingleton.Instance.FindPackage(packageIdentity.AppName), cancellationToken).ConfigureAwait(false);

            if (find == null)
            {
                return new List<PackageEntry>();
            }
        }

        var dependencies = find.Dependencies;

        var list = new List<PackageEntry>();

        foreach (var dep in dependencies.Where(p => p.IsOptional))
        {
            var converted = await dep.ToPackageEntry(false, cancellationToken).ConfigureAwait(false);
            if (converted != null)
            {
                list.Add(converted);
            }
        }

        return list;
    }

    public async Task<AppxPackage> GetByIdentity(string packageName, PackageQuerySourceType mode = PackageQuerySourceType.InstalledForCurrentUser, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
    {
        switch (mode)
        {
            case PackageQuerySourceType.Installed:
            case PackageQuerySourceType.InstalledForCurrentUser:
            case PackageQuerySourceType.InstalledForAllUsers:
                break;
            default:
                throw new NotSupportedException($"Mode {mode} is not supported by this method.");
        }

        using var reader = new PackageIdentityFileReaderAdapter(PackageManagerSingleton.Instance, mode == PackageQuerySourceType.InstalledForCurrentUser ? PackageInstallationContext.CurrentUser : PackageInstallationContext.AllUsers, packageName);

        var manifestReader = new AppxManifestReaderWithDependencyResolving(new AppxManifestReader());
        // ReSharper disable once AccessToDisposedClosure
        var package = await manifestReader.Read(reader, cancellationToken).ConfigureAwait(false);
        return package;
    }

    public async Task<AppxPackage> GetByManifestPath(string manifestPath, PackageQuerySourceType mode = PackageQuerySourceType.InstalledForCurrentUser, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
    {
        switch (mode)
        {
            case PackageQuerySourceType.Installed:
            case PackageQuerySourceType.InstalledForCurrentUser:
            case PackageQuerySourceType.InstalledForAllUsers:
                break;
            default:
                throw new NotSupportedException($"Mode {mode} is not supported by this method.");
        }

        using IAppxFileReader reader = new FileInfoFileReaderAdapter(manifestPath);
        var manifestReader = new AppxManifestReaderWithDependencyResolving(new AppxManifestReader());
        // ReSharper disable once AccessToDisposedClosure
        var package = await manifestReader.Read(reader,  cancellationToken).ConfigureAwait(false);
        return package;
    }

    public async Task<PackageEntry> GetInstalledPackage(string fullName, PackageQuerySourceType mode = PackageQuerySourceType.Installed, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
    {
        switch (mode)
        {
            case PackageQuerySourceType.Installed:
            case PackageQuerySourceType.InstalledForCurrentUser:
            case PackageQuerySourceType.InstalledForAllUsers:
                break;
            default:
                throw new NotSupportedException($"Mode {mode} is not supported by this method.");
        }

        var pkgs = await QueryInstalledPackages(fullName, mode, cancellationToken).ConfigureAwait(false);
        return pkgs.FirstOrDefault();
    }

    public async Task<PackageEntry> GetInstalledPackageByFamilyName(string familyName, PackageQuerySourceType mode = PackageQuerySourceType.Installed, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
    {
        switch (mode)
        {
            case PackageQuerySourceType.Installed:
            case PackageQuerySourceType.InstalledForCurrentUser:
            case PackageQuerySourceType.InstalledForAllUsers:
                break;
            default:
                throw new NotSupportedException($"Mode {mode} is not supported by this method.");
        }

        var isAdmin = await UserHelper.IsAdministratorAsync(cancellationToken).ConfigureAwait(false);
        if (mode == PackageQuerySourceType.Installed)
        {
            mode = isAdmin ? PackageQuerySourceType.InstalledForAllUsers : PackageQuerySourceType.InstalledForCurrentUser;
        }

        Package found;

        switch (mode)
        {
            case PackageQuerySourceType.InstalledForCurrentUser:
                found = await Task.Run(() => PackageManagerSingleton.Instance.FindPackagesForUser(string.Empty, familyName).FirstOrDefault(), cancellationToken).ConfigureAwait(false);
                break;
            case PackageQuerySourceType.InstalledForAllUsers:
                found = await Task.Run(() => PackageManagerSingleton.Instance.FindPackages(familyName).FirstOrDefault(), cancellationToken).ConfigureAwait(false);
                break;
            default:
                throw new InvalidOperationException();
        }

        if (found == null)
        {
            return null;
        }

        return await GetInstalledPackage(found.Id.FullName, mode, cancellationToken).ConfigureAwait(false);
    }

    private async Task<List<PackageEntry>> QueryInstalledPackages(string packageName, PackageQuerySourceType mode, CancellationToken cancellationToken, IProgress<ProgressData> progress = default)
    {
        if (mode == PackageQuerySourceType.Installed)
        {
            var isAdmin = await UserHelper.IsAdministratorAsync(cancellationToken).ConfigureAwait(false);
            mode = isAdmin ? PackageQuerySourceType.InstalledForAllUsers : PackageQuerySourceType.InstalledForCurrentUser;
        }

        progress?.Report(new ProgressData(0, Resources.Localization.Packages_GettingApps));

        var provisioning = Task.Run(
            () =>
            {
                if (!UserHelper.IsAdministrator())
                {
                    return null;
                }

                return new HashSet<string>(PackageManagerSingleton.Instance.FindProvisionedPackages().Select(p => p.Id.FullName.Replace("~", string.Empty)));
            },
            cancellationToken);
        
        progress?.Report(new ProgressData(5, Resources.Localization.Packages_GettingApps));

        IList<Package> allPackages;

        if (string.IsNullOrEmpty(packageName))
        {
            Logger.Info().WriteLine("Getting all packages by find mode = '{0}'", mode);
            switch (mode)
            {
                case PackageQuerySourceType.InstalledForCurrentUser:
                    allPackages = await Task.Run(() => PackageManagerSingleton.Instance.FindPackagesForUserWithPackageTypes(string.Empty, PackageTypes.Framework | PackageTypes.Main | PackageTypes.Optional).ToList(), cancellationToken).ConfigureAwait(false);
                    break;
                case PackageQuerySourceType.InstalledForAllUsers:
                    allPackages = await Task.Run(() => PackageManagerSingleton.Instance.FindPackagesWithPackageTypes(PackageTypes.Framework | PackageTypes.Main | PackageTypes.Optional).ToList(), cancellationToken).ConfigureAwait(false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }
        else
        {
            allPackages = new List<Package>();
            Logger.Info().WriteLine("Getting package name '{0}' by find mode = '{1}'", packageName, mode);

            switch (mode)
            {
                case PackageQuerySourceType.InstalledForCurrentUser:
                {
                    var pkg = PackageManagerSingleton.Instance.FindPackageForUser(string.Empty, packageName);

                    if (pkg != null)
                    {
                        allPackages.Add(pkg);
                    }

                    break;
                }

                case PackageQuerySourceType.InstalledForAllUsers:
                {
                    var pkg = PackageManagerSingleton.Instance.FindPackage(packageName);
                        
                    if (pkg != null)
                    {
                        allPackages.Add(pkg);
                    }

                    break;
                }

                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }

        progress?.Report(new ProgressData(30, Resources.Localization.Packages_GettingApps));

        var total = 10.0;
        var single = allPackages.Count == 0 ? 0.0 : 90.0 / allPackages.Count;
        
        var config = await configurationService.GetCurrentConfigurationAsync(true, cancellationToken).ConfigureAwait(false);

        int maxSimultaneousTasks;
        if (config.Advanced?.DisableTasksForGetPackages == false || config.Advanced?.MaxTasksForGetPackages < 2)
        {
            maxSimultaneousTasks = 1;
        }
        else if (config.Advanced?.MaxTasksForGetPackages == null)
        {
            maxSimultaneousTasks = 20;
        }
        else
        {
            maxSimultaneousTasks = Math.Max(config.Advanced.MaxTasksForGetPackages.Value, 1);
        }

        var tasks = new HashSet<Task<PackageEntry>>(maxSimultaneousTasks);

        var sw = new Stopwatch();
        sw.Start();

        var list = new List<PackageEntry>(allPackages.Count);
        foreach (var item in allPackages)
        {
            total += single;
            progress?.Report(new ProgressData((int)total, Resources.Localization.Packages_GettingApps));

            cancellationToken.ThrowIfCancellationRequested();

            if (tasks.Count >= maxSimultaneousTasks)
            {
                var awaited = await Task.WhenAny(tasks).ConfigureAwait(false);
                tasks.Remove(awaited);
                
                if (awaited.Result != null)
                {
                    list.Add(awaited.Result);
                }
            }

            tasks.Add(item.ToPackageEntry(false, cancellationToken));
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);
        list.AddRange(tasks.Where(t => t.Result != null).Select(t => t.Result));
        
        var provisionIds = await provisioning.ConfigureAwait(false);
        if (provisionIds != null)
        {
            foreach (var item in list.Where(p => provisionIds.Contains(p.PackageFullName)))
            {
                item.IsProvisioned = true;
            }
        }

        sw.Stop();
        Logger.Info().WriteLine("Returning {0} packages (the operation took {1})…", list.Count, sw.Elapsed);
        return list;
    }
    
    private static string SidToAccountName(string sidString)
    {
        var sid = new SecurityIdentifier(sidString);
        try
        {
            var account = (NTAccount)sid.Translate(typeof(NTAccount));
            return account.ToString();
        }
        catch (IdentityNotMappedException)
        {
            return sidString;
        }
    }

    public struct MsixPackageVisuals
    {
        public MsixPackageVisuals(string displayName, string displayPublisherName, string logoRelativePath, string description, string color, MsixApplicationType packageType)
        {
            DisplayName = displayName;
            DisplayPublisherName = displayPublisherName;
            LogoRelativePath = logoRelativePath;
            Description = description;
            Color = color;
            PackageType = packageType;
        }

        public string Description;
        public string DisplayName;
        public string DisplayPublisherName;
        public string LogoRelativePath;
        public string Color;
        public MsixApplicationType PackageType;
    }
}