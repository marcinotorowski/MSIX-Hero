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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Management.Deployment;
using Otor.MsixHero.Appx.Diagnostic.Registry;
using Otor.MsixHero.Appx.Diagnostic.Registry.Enums;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;
using Otor.MsixHero.Appx.Packaging.Installation.Enums;
using Otor.MsixHero.Appx.Packaging.Manifest;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities.Summary;
using Otor.MsixHero.Appx.Packaging.Manifest.FileReaders;
using Otor.MsixHero.Appx.Users;
using Otor.MsixHero.Infrastructure.Helpers;
using Otor.MsixHero.Infrastructure.Logging;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.Services;

namespace Otor.MsixHero.Appx.Packaging.Installation
{
    [SuppressMessage("ReSharper", "UnusedVariable")]
    public class AppxPackageQuery : IAppxPackageQuery
    {
        public static Lazy<PackageManager> PackageManager = new(() => new PackageManager(), true);

        private static readonly ILog Logger = LogManager.GetLogger();

        private readonly IRegistryManager registryManager;
        private readonly IConfigurationService configurationService;

        public AppxPackageQuery(IRegistryManager registryManager, IConfigurationService configurationService)
        {
            this.registryManager = registryManager;
            this.configurationService = configurationService;
        }

        public Task<List<User>> GetUsersForPackage(InstalledPackage package, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            return GetUsersForPackage(package.Name, cancellationToken, progress);
        }

        public async Task<List<User>> GetUsersForPackage(string packageName, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            Logger.Info("Getting users who installed package {0}...", packageName);
            if (!await UserHelper.IsAdministratorAsync(cancellationToken).ConfigureAwait(false))
            {
                Logger.Info("The user is not administrator. Returning an empty list.");
                return new List<User>();
            }

            var result = await Task.Run(
                () =>
                {
                    var list = PackageManager.Value.FindUsers(packageName).Select(u => new User(SidToAccountName(u.UserSecurityId))).ToList();
                    return list;
                },
                cancellationToken).ConfigureAwait(false);

            Logger.Info("Returning {0} users...", result.Count);
            return result;
        }
        
        public Task<List<InstalledPackage>> GetInstalledPackages(PackageFindMode mode = PackageFindMode.Auto, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            return GetInstalledPackages(null, mode, cancellationToken, progress);
        }

        public async Task<List<InstalledPackage>> GetModificationPackages(string packageFullName, PackageFindMode mode = PackageFindMode.Auto, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
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
                    return new List<InstalledPackage>();
                }
            }

            var dependencies = find.Dependencies;

            var list = new List<InstalledPackage>();

            foreach (var dep in dependencies.Where(p => p.IsOptional))
            {
                var converted = await ConvertFrom(dep, cancellationToken).ConfigureAwait(false);
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
        
        private async Task<List<InstalledPackage>> GetInstalledPackages(string packageName, PackageFindMode mode, CancellationToken cancellationToken, IProgress<ProgressData> progress = default)
        {
            var list = new List<InstalledPackage>();
            var provisioned = new HashSet<string>();

            var isAdmin = await UserHelper.IsAdministratorAsync(cancellationToken).ConfigureAwait(false);
            if (mode == PackageFindMode.Auto)
            {
                mode = isAdmin ? PackageFindMode.AllUsers : PackageFindMode.CurrentUser;
            }

            progress?.Report(new ProgressData(0, "Getting installed apps..."));

            if (isAdmin)
            {
                Logger.Info("Getting provisioned packages...");
                var tempFile = Path.GetTempFileName();
                try
                {
                    var cmd = "(Get-AppxProvisionedPackage -Online).PackageName | Out-File '" + tempFile + "'";
                    var proc = new ProcessStartInfo("powershell.exe", "-NoLogo -WindowStyle Hidden -Command \"&{ " + cmd + "}\"")
                    {
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    Logger.Debug("Executing powershell.exe " + "-Command \"&{ " + cmd + "}\"");
                    var p = Process.Start(proc);
                    if (p == null)
                    {
                        Logger.Error("Could not get the list of provisioned apps.");
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

            progress?.Report(new ProgressData(5, "Getting installed apps..."));

            IList<Package> allPackages;

            if (string.IsNullOrEmpty(packageName))
            {
                Logger.Info("Getting all packages by find mode = '{0}'", mode);
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
                Logger.Info("Getting package name '{0}' by find mode = '{1}'", packageName, mode);
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

            progress?.Report(new ProgressData(30, "Getting installed apps..."));

            var total = 10.0;
            var single = allPackages.Count == 0 ? 0.0 : 90.0 / allPackages.Count;

            var all = allPackages.Count;

            var tasks = new HashSet<Task<InstalledPackage>>();
            var config = await configurationService.GetCurrentConfigurationAsync(true, cancellationToken).ConfigureAwait(false);

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
                progress?.Report(new ProgressData((int)total, $"Getting installed apps..."));

                cancellationToken.ThrowIfCancellationRequested();

                if (tasks.Count >= maxThreads)
                {
                    var awaited = await Task.WhenAny(tasks).ConfigureAwait(false);
                    tasks.Remove(awaited);

                    var converted = await awaited.ConfigureAwait(false);
                    if (converted != null)
                    {
                        converted.Context = mode == PackageFindMode.CurrentUser ? PackageContext.CurrentUser : PackageContext.AllUsers;
                        if (provisioned.Contains(converted.PackageId))
                        {
                            converted.IsProvisioned = true;
                        }

                        list.Add(converted);
                    }
                }

                tasks.Add(ConvertFrom(item, cancellationToken, progress));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
            foreach (var item in tasks)
            {
                var converted = await item.ConfigureAwait(false);

                if (converted != null)
                {
                    converted.Context = mode == PackageFindMode.CurrentUser ? PackageContext.CurrentUser : PackageContext.AllUsers;
                    if (provisioned.Contains(converted.PackageId))
                    {
                        converted.IsProvisioned = true;
                    }

                    list.Add(converted);
                }
            }

            sw.Stop();

            Logger.Info("Returning {0} packages (the operation took {1})...", list.Count, sw.Elapsed);
            return list;
        }

        private async Task<InstalledPackage> ConvertFrom(Package item, CancellationToken cancellationToken, IProgress<ProgressData> progress = default)
        {
            Logger.Debug("Getting details about package {0}...", item.Id.Name);
            string installLocation;
            DateTime installDate;
            try
            {
                installLocation = item.InstalledLocation?.Path;
            }
            catch (Exception e)
            {
                Logger.Warn(e, "Installed location for package {0} is invalid. This may be expected for some installed packages.", item.Id.Name);
                installLocation = null;
            }

            if (installLocation != null)
            {
                try
                {
                    installDate = item.InstalledDate.LocalDateTime;
                }
                catch (Exception e)
                {
                    Logger.Warn(e, "Installed date for package {0} is invalid. This may be expected for some installed packages.", item.Id.Name);
                    installDate = DateTime.MinValue;
                }
            }
            else
            {
                installDate = DateTime.MinValue;
            }

            MsixPackageVisuals details;
            RegistryMountState hasRegistry;

            if (installLocation == null)
            {
                hasRegistry = RegistryMountState.NotApplicable;
                details = new MsixPackageVisuals(item.Id.Name, item.Id.Publisher, null, null, "#000000", 0);
            }
            else
            {
                details = await GetVisualsFromManifest(installLocation, cancellationToken).ConfigureAwait(false);
                hasRegistry = await registryManager.GetRegistryMountState(installLocation, item.Id.Name, cancellationToken, progress).ConfigureAwait(false);
            }

            var pkg = new InstalledPackage
            {
                DisplayName = details.DisplayName,
                Name = item.Id.Name,
                Image = details.Logo,
                PackageId = item.Id.FullName,
                InstallLocation = installLocation,
                PackageFamilyName = item.Id.FamilyName,
                Description = details.Description,
                DisplayPublisherName = details.DisplayPublisherName,
                Publisher = item.Id.Publisher,
                Architecture = item.Id.Architecture.ToString(),
                IsFramework = item.IsFramework,
                IsOptional = item.IsOptional,
                TileColor = details.Color,
                PackageType = details.PackageType,
                Version = new Version(item.Id.Version.Major, item.Id.Version.Minor, item.Id.Version.Build, item.Id.Version.Revision),
                SignatureKind = Convert(item.SignatureKind),
                HasRegistry = hasRegistry,
                InstallDate = installDate,
                AppInstallerUri = item.GetAppInstallerInfo()?.Uri
            };

            if (pkg.Architecture[0] == 'X')
            {
                pkg.Architecture = "x" + pkg.Architecture.Substring(1);
            }

            if (installLocation != null && (pkg.DisplayName?.StartsWith("ms-resource:", StringComparison.Ordinal) ??
                                            pkg.DisplayPublisherName?.StartsWith("ms-resource:", StringComparison.Ordinal) ??
                                            pkg.Description?.StartsWith("ms-resource:", StringComparison.Ordinal) == true))
            {
                var priFile = Path.Combine(installLocation, "resources.pri");

                pkg.DisplayName = StringLocalizer.Localize(priFile, pkg.Name, pkg.PackageId, pkg.DisplayName);
                pkg.DisplayPublisherName = StringLocalizer.Localize(priFile, pkg.Name, pkg.PackageId, pkg.DisplayPublisherName);
                pkg.Description = StringLocalizer.Localize(priFile, pkg.Name, pkg.PackageId, pkg.Description);

                if (string.IsNullOrEmpty(pkg.DisplayName))
                {
                    pkg.DisplayName = pkg.Name;
                }

                if (string.IsNullOrEmpty(pkg.DisplayPublisherName))
                {
                    pkg.DisplayPublisherName = pkg.Publisher;
                }
            }

            return pkg;
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

        private static async Task<MsixPackageVisuals> GetVisualsFromManifest(string installLocation, CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                var reader = await AppxManifestSummaryReader.FromInstallLocation(installLocation).ConfigureAwait(false);
                var logo = Path.Combine(installLocation, reader.Logo);

                if (File.Exists(Path.Combine(installLocation, logo)))
                {
                    return new MsixPackageVisuals(
                        reader.DisplayName,
                        reader.DisplayPublisher,
                        logo,
                        reader.Description,
                        reader.AccentColor,
                        reader.PackageType);
                }

                var extension = Path.GetExtension(logo);
                var baseName = Path.GetFileNameWithoutExtension(logo);
                var baseFolder = Path.GetDirectoryName(logo);

                logo = null;

                // ReSharper disable once AssignNullToNotNullAttribute
                var dirInfo = new DirectoryInfo(Path.Combine(installLocation, baseFolder));
                if (dirInfo.Exists)
                {
                    var found = dirInfo.EnumerateFiles(baseName + "*" + extension).FirstOrDefault();
                    if (found != null)
                    {
                        logo = found.FullName;
                    }
                }

                cancellationToken.ThrowIfCancellationRequested();
                return new MsixPackageVisuals(reader.DisplayName, reader.DisplayPublisher, logo, reader.Description, reader.AccentColor, reader.PackageType);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception)
            {
                return new MsixPackageVisuals();
            }
        }

        private static SignatureKind Convert(PackageSignatureKind signatureKind)
        {
            switch (signatureKind)
            {
                case PackageSignatureKind.None:
                    return SignatureKind.Unsigned;
                case PackageSignatureKind.Developer:
                    return SignatureKind.Developer;
                case PackageSignatureKind.Enterprise:
                    return SignatureKind.Enterprise;
                case PackageSignatureKind.Store:
                    return SignatureKind.Store;
                case PackageSignatureKind.System:
                    return SignatureKind.System;
                default:
                    throw new NotSupportedException();
            }
        }

        public struct MsixPackageVisuals
        {
            public MsixPackageVisuals(string displayName, string displayPublisherName, string logo, string description, string color, MsixPackageType packageType)
            {
                DisplayName = displayName;
                DisplayPublisherName = displayPublisherName;
                Logo = logo;
                Description = description;
                Color = color;
                PackageType = packageType;
            }

            public string Description;
            public string DisplayName;
            public string DisplayPublisherName;
            public string Logo;
            public string Color;
            public MsixPackageType PackageType;
        }
    }
}