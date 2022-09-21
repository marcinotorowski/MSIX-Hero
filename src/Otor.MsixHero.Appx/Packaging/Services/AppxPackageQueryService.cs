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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Management.Deployment;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;
using Otor.MsixHero.Appx.Packaging.Installation.Enums;
using Otor.MsixHero.Appx.Packaging.Manifest;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;
using Otor.MsixHero.Appx.Packaging.Manifest.FileReaders;
using Otor.MsixHero.Appx.Users;
using Otor.MsixHero.Infrastructure.Helpers;
using Dapplo.Log;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.Services;

namespace Otor.MsixHero.Appx.Packaging.Services;

public class AppxPackageQueryService : IAppxPackageQueryService
{
    public static Lazy<PackageManager> PackageManager = new(() => new PackageManager(), true);

    private static readonly LogSource Logger = new();
    private readonly IConfigurationService _configurationService;

    public AppxPackageQueryService(IConfigurationService configurationService)
    {
        this._configurationService = configurationService;
    }

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
                var list = PackageManager.Value.FindUsers(packageName).Select(u => new User(SidToAccountName(u.UserSecurityId))).ToList();
                return list;
            },
            cancellationToken).ConfigureAwait(false);

        Logger.Info().WriteLine("Returning {0} users…", result.Count);
        return result;
    }

    public Task<List<PackageEntry>> GetInstalledPackages(PackageFindMode mode = PackageFindMode.Auto, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
    {
        return QueryInstalledPackages(null, mode, cancellationToken, progress);
    }

    public async Task<List<PackageEntry>> GetModificationPackages(string packageFullName, PackageFindMode mode = PackageFindMode.Auto, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
    {
        if (mode == PackageFindMode.Auto)
        {
            mode = await UserHelper.IsAdministratorAsync(cancellationToken).ConfigureAwait(false) ? PackageFindMode.AllUsers : PackageFindMode.CurrentUser;
        }

        var find = await Task.Run(() => mode == PackageFindMode.CurrentUser ? PackageManager.Value.FindPackageForUser(string.Empty, packageFullName) : PackageManager.Value.FindPackage(packageFullName), cancellationToken).ConfigureAwait(false);
        if (find == null)
        {
            var packageIdentity = PackageIdentity.FromFullName(packageFullName);
            find = await Task.Run(() => mode == PackageFindMode.CurrentUser ? PackageManager.Value.FindPackageForUser(string.Empty, packageIdentity.AppName) : PackageManager.Value.FindPackage(packageIdentity.AppName), cancellationToken).ConfigureAwait(false);

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
            list.Add(converted);
        }

        return list;
    }

    public async Task<AppxPackage> GetByIdentity(string packageName, PackageFindMode mode = PackageFindMode.CurrentUser, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
    {
        using var reader = new PackageIdentityFileReaderAdapter(mode == PackageFindMode.CurrentUser ? PackageContext.CurrentUser : PackageContext.AllUsers, packageName);
        var manifestReader = new AppxManifestReader();
        // ReSharper disable once AccessToDisposedClosure
        var package = await Task.Run(() => manifestReader.Read(reader, true, cancellationToken), cancellationToken).ConfigureAwait(false);
        return package;
    }

    public async Task<AppxPackage> GetByManifestPath(string manifestPath, PackageFindMode mode = PackageFindMode.CurrentUser, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
    {
        using IAppxFileReader reader = new FileInfoFileReaderAdapter(manifestPath);
        var manifestReader = new AppxManifestReader();
        // ReSharper disable once AccessToDisposedClosure
        var package = await Task.Run(() => manifestReader.Read(reader, true, cancellationToken), cancellationToken).ConfigureAwait(false);
        return package;
    }

    public async Task<PackageEntry> GetInstalledPackage(string fullName, PackageFindMode mode = PackageFindMode.Auto, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
    {
        var pkgs = await QueryInstalledPackages(fullName, mode, cancellationToken).ConfigureAwait(false);
        return pkgs.FirstOrDefault();
    }

    public async Task<PackageEntry> GetInstalledPackageByFamilyName(string familyName, PackageFindMode mode = PackageFindMode.Auto, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
    {
        var isAdmin = await UserHelper.IsAdministratorAsync(cancellationToken).ConfigureAwait(false);
        if (mode == PackageFindMode.Auto)
        {
            mode = isAdmin ? PackageFindMode.AllUsers : PackageFindMode.CurrentUser;
        }

        Package found;

        switch (mode)
        {
            case PackageFindMode.CurrentUser:
                found = await Task.Run(() => PackageManager.Value.FindPackagesForUser(string.Empty, familyName).FirstOrDefault(), cancellationToken).ConfigureAwait(false);
                break;
            case PackageFindMode.AllUsers:
                found = await Task.Run(() => PackageManager.Value.FindPackages(familyName).FirstOrDefault(), cancellationToken).ConfigureAwait(false);
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

    private async Task<List<PackageEntry>> QueryInstalledPackages(string packageName, PackageFindMode mode, CancellationToken cancellationToken, IProgress<ProgressData> progress = default)
    {
        var list = new List<PackageEntry>();
        var provisioned = new HashSet<string>();

        var isAdmin = await UserHelper.IsAdministratorAsync(cancellationToken).ConfigureAwait(false);
        if (mode == PackageFindMode.Auto)
        {
            mode = isAdmin ? PackageFindMode.AllUsers : PackageFindMode.CurrentUser;
        }

        progress?.Report(new ProgressData(0, Resources.Localization.Packages_GettingApps));

        if (isAdmin)
        {
            Logger.Info().WriteLine("Getting provisioned packages…");
            var tempFile = Path.GetTempFileName();
            try
            {
                var cmd = "(Get-AppxProvisionedPackage -Online).PackageName | Out-File '" + tempFile + "'";
                var proc = new ProcessStartInfo("powershell.exe", "-NoLogo -WindowStyle Hidden -Command \"&{ " + cmd + "}\"")
                {
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                Logger.Debug().WriteLine("Executing powershell.exe " + "-Command \"&{ " + cmd + "}\"");
                var p = Process.Start(proc);
                if (p == null)
                {
                    Logger.Error().WriteLine("Could not get the list of provisioned apps.");
                }
                else
                {
                    await p.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
                    foreach (var line in await File.ReadAllLinesAsync(tempFile, cancellationToken).ConfigureAwait(false))
                    {
                        provisioned.Add(line.Replace("~", string.Empty));
                    }
                }
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        progress?.Report(new ProgressData(5, Resources.Localization.Packages_GettingApps));

        IList<Package> allPackages;

        if (string.IsNullOrEmpty(packageName))
        {
            Logger.Info().WriteLine("Getting all packages by find mode = '{0}'", mode);
            switch (mode)
            {
                case PackageFindMode.CurrentUser:
                    allPackages = await Task.Run(() => PackageManager.Value.FindPackagesForUserWithPackageTypes(string.Empty, PackageTypes.Framework | PackageTypes.Main | PackageTypes.Optional).ToList(), cancellationToken).ConfigureAwait(false);
                    break;
                case PackageFindMode.AllUsers:
                    allPackages = await Task.Run(() => PackageManager.Value.FindPackagesWithPackageTypes(PackageTypes.Framework | PackageTypes.Main | PackageTypes.Optional).ToList(), cancellationToken).ConfigureAwait(false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }
        else
        {
            Logger.Info().WriteLine("Getting package name '{0}' by find mode = '{1}'", packageName, mode);
            switch (mode)
            {
                case PackageFindMode.CurrentUser:
                {
                    var pkg = await Task.Run(() => PackageManager.Value.FindPackageForUser(string.Empty, packageName), cancellationToken).ConfigureAwait(false);

                    allPackages = new List<Package>();
                    if (pkg != null)
                    {
                        allPackages.Add(pkg);
                    }

                    break;
                }

                case PackageFindMode.AllUsers:
                {
                    var pkg = await Task.Run(() => PackageManager.Value.FindPackage(packageName), cancellationToken).ConfigureAwait(false);

                    allPackages = new List<Package>();
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
        
        var tasks = new HashSet<Task<PackageEntry>>();
        var config = await this._configurationService.GetCurrentConfigurationAsync(true, cancellationToken).ConfigureAwait(false);

        int maxThreads;
        if (config.Advanced?.DisableMultiThreadingForGetPackages == false || config.Advanced?.MaxThreadsForGetPackages < 2)
        {
            maxThreads = 1;
        }
        else if (config.Advanced?.MaxThreadsForGetPackages == null)
        {
            maxThreads = Environment.ProcessorCount;
        }
        else
        {
            maxThreads = Math.Min(config.Advanced?.MaxThreadsForGetPackages ?? 1, Environment.ProcessorCount);
        }

        var sw = new Stopwatch();
        sw.Start();

        foreach (var item in allPackages)
        {
            total += single;
            progress?.Report(new ProgressData((int)total, Resources.Localization.Packages_GettingApps));

            cancellationToken.ThrowIfCancellationRequested();

            if (tasks.Count >= maxThreads)
            {
                var awaited = await Task.WhenAny(tasks).ConfigureAwait(false);
                tasks.Remove(awaited);

                var converted = await awaited.ConfigureAwait(false);
                if (converted != null)
                {
                    if (provisioned.Contains(converted.PackageFullName))
                    {
                        converted.IsProvisioned = true;
                    }

                    list.Add(converted);
                }
            }

            tasks.Add(item.ToPackageEntry(false, cancellationToken));
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);
        foreach (var item in tasks)
        {
            var converted = await item.ConfigureAwait(false);

            if (converted != null)
            {
                if (provisioned.Contains(converted.PackageFullName))
                {
                    converted.IsProvisioned = true;
                }

                list.Add(converted);
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
        public MsixPackageVisuals(string displayName, string displayPublisherName, string logoRelativePath, string description, string color, MsixPackageType packageType)
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
        public MsixPackageType PackageType;
    }
}