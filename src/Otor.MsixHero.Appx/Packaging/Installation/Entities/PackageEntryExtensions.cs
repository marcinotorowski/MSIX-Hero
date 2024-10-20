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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Dapplo.Log;
using Otor.MsixHero.Appx.Common;
using Otor.MsixHero.Appx.Diagnostic.RunningDetector;
using Otor.MsixHero.Appx.Packaging.Manifest;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities.Summary;
using Otor.MsixHero.Appx.Packaging.Manifest.Enums;
using Otor.MsixHero.Appx.Packaging.Services;
using Otor.MsixHero.Appx.Reader;
using Otor.MsixHero.Infrastructure.Helpers;

namespace Otor.MsixHero.Appx.Packaging.Installation.Entities;

public static class PackageEntryExtensions
{
    private static readonly LogSource Logger = new();

    public static async Task<IList<PackageEntry>> FromFilePaths(
        IEnumerable<string> filePaths,
        PackageFindMode packageContextMode = PackageFindMode.Auto,
        bool checkIfRunning = false,
        CancellationToken cancellationToken = default)
    {
        var packages = new List<PackageEntry>();

        foreach (var package in filePaths)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var pkg = await FromFilePath(package, packageContextMode, false, cancellationToken).ConfigureAwait(false);
            packages.Add(pkg);
        }

        if (checkIfRunning)
        {
            var runningDetector = new RunningAppsDetector();
            var running = runningDetector.GetCurrentlyRunningPackageFamilyNames();
            if (running.Any())
            {
                foreach (var pkg in packages)
                {
                    pkg.IsRunning = running.Contains(pkg.PackageFamilyName);
                }
            }
        }

        return packages;
    }
    
    public static async Task<PackageEntry> FromReader(
        IAppxFileReader appxPackageReader,
        PackageFindMode packageContextMode = PackageFindMode.Auto,
        bool checkIfRunning = false,
        CancellationToken cancellationToken = default)
    {
        var manifestReader = new AppxManifestReader();
        var appxPackage = await manifestReader.Read(appxPackageReader, cancellationToken).ConfigureAwait(false);

        Logger.Debug().WriteLine("Getting details about package {0} from its manifest information…", appxPackage.FullName);

        var entry = new PackageEntry
        {
            Name = appxPackage.Name,
            PackageFullName = appxPackage.FullName,
            ManifestPath = appxPackage.PackagePath,
            PackageFamilyName = appxPackage.FamilyName,
            DisplayPublisherName = appxPackage.PublisherDisplayName,
            Description = appxPackage.Description,
            Publisher = appxPackage.Publisher,
            ResourceId = appxPackage.ResourceId,
            Architecture = appxPackage.ProcessorArchitecture,
            IsFramework = appxPackage.IsFramework,
            IsOptional = appxPackage.IsOptional,
            Version = Version.TryParse(appxPackage.Version, out var parsedVersion) ? parsedVersion : throw new ArgumentException(@"Wrong version value.", nameof(appxPackage)),
            SignatureKind = SignatureKind.Unknown,
            IsRunning = false,
            IsProvisioned = false,
            DisplayName = appxPackage.DisplayName,
            AppInstallerUri = default,
            InstallDate = default,
            InstallDirPath = default
        };

        if (packageContextMode == PackageFindMode.Auto)
        {
            if (await UserHelper.IsAdministratorAsync(cancellationToken).ConfigureAwait(false))
            {
                packageContextMode = PackageFindMode.AllUsers;
            }
            else
            {
                packageContextMode = PackageFindMode.CurrentUser;
            }
        }

        Package installPkg;
        switch (packageContextMode)
        {
            case PackageFindMode.CurrentUser:
                installPkg = PackageManagerSingleton.Instance.FindPackageForUser(string.Empty, appxPackage.FullName);
                if (installPkg == null)
                {
                    var identity = PackageIdentity.FromFullName(appxPackage.FullName);
                    if (string.IsNullOrWhiteSpace(identity.ResourceId))
                    {
                        identity.ResourceId = "Neutral";
                        installPkg = PackageManagerSingleton.Instance.FindPackageForUser(string.Empty, identity.ToString());
                    }
                }
                break;

            case PackageFindMode.AllUsers:
                installPkg = PackageManagerSingleton.Instance.FindPackage(appxPackage.FullName);
                if (installPkg == null)
                {
                    var identity = PackageIdentity.FromFullName(appxPackage.FullName);
                    if (string.IsNullOrWhiteSpace(identity.ResourceId))
                    {
                        identity.ResourceId = "Neutral";
                        installPkg = PackageManagerSingleton.Instance.FindPackage(identity.ToString());
                    }
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(packageContextMode), packageContextMode, null);
        }

        var details = await AppxManifestSummaryReader.FromMsix(appxPackageReader).ConfigureAwait(false);

        if (details.Logo != null)
        {
            if (appxPackageReader is IAppxDiskFileReader diskReader)
            {
                entry.ImagePath = Path.Combine(diskReader.RootDirectory, details.Logo);
            }
            else
            {
                if (appxPackageReader.FileExists(details.Logo))
                {
                    await using var s = appxPackageReader.GetFile(details.Logo);
                    await using var m = new MemoryStream();
                    await s.CopyToAsync(m, cancellationToken).ConfigureAwait(false);
                    m.Seek(0, SeekOrigin.Begin);
                    entry.ImageContent = m.ToArray();
                }
            }
        }

        if (installPkg != null)
        {
            entry.InstallDirPath = ExceptionGuard.Guard(() => installPkg.InstalledLocation?.Path);
            entry.InstallDate = ExceptionGuard.Guard(() => installPkg.InstalledDate.LocalDateTime);
            entry.AppInstallerUri = installPkg.GetAppInstallerInfo()?.Uri;
            entry.SignatureKind = ConvertSignatureKind(installPkg.SignatureKind);
        }
        else
        {
            entry.SignatureKind = SignatureKind.Unknown;
        }

        if (checkIfRunning)
        {
            var detector = new RunningAppsDetector();
            var getRunning = detector.GetCurrentlyRunningPackageFamilyNames();
            entry.IsRunning = getRunning.Contains(entry.PackageFamilyName);
        }

        return entry;
    }

    public static async Task<PackageEntry> FromFilePath(
        string fullFilePath,
        PackageFindMode packageContextMode = PackageFindMode.Auto,
        bool checkIfRunning = false,
        CancellationToken cancellationToken = default)
    {
        using var appxPackageReader = FileReaderFactory.CreateFileReader(fullFilePath);
        return await FromReader(appxPackageReader, packageContextMode, checkIfRunning, cancellationToken).ConfigureAwait(false);
    }
    
    public static async Task<PackageEntry> ToPackageEntry(this Package originalPackageEntry, bool checkIfRunning = false, CancellationToken cancellationToken = default)
    {
        Logger.Debug().WriteLine("Getting details about package {0}…", originalPackageEntry.Id.Name);
        string installLocation;
        DateTime installDate;
        try
        {
            installLocation = originalPackageEntry.InstalledLocation?.Path;
        }
        catch (FileNotFoundException e)
        {
            Logger.Warn().WriteLine("Installed location for package {0} is invalid. Package will be ignored.", originalPackageEntry.Id.Name);
            Logger.Verbose().WriteLine(e);
            return null;
        }

        if (installLocation != null)
        {
            try
            {
                installDate = originalPackageEntry.InstalledDate.LocalDateTime;
            }
            catch (COMException e)
            {
                if (e.ErrorCode != -2147023728)
                {
                    throw;
                }

                Logger.Warn().WriteLine("Installed date for package {0} is invalid. This may be expected for some installed packages.", originalPackageEntry.Id.Name);
                Logger.Verbose().WriteLine(e);
                installDate = DateTime.MinValue;
            }
        }
        else
        {
            installDate = DateTime.MinValue;
        }

        AppxPackageQueryService.MsixPackageVisuals details;

        if (installLocation == null)
        {
            details = new AppxPackageQueryService.MsixPackageVisuals(originalPackageEntry.Id.Name, originalPackageEntry.Id.Publisher, null, null, "#000000", 0);
        }
        else
        {
            details = await GetVisualsFromManifest(installLocation, cancellationToken).ConfigureAwait(false);
        }

        var pkg = new PackageEntry
        {
            Name = originalPackageEntry.Id.Name,
            ImagePath = installLocation == null || details.LogoRelativePath == null ? null : Path.Combine(installLocation, details.LogoRelativePath),
            PackageFullName = originalPackageEntry.Id.FullName,
            InstallDirPath = installLocation,
            ManifestPath = installLocation == null ? null : Path.Combine(installLocation, AppxFileConstants.AppxManifestFile),
            PackageFamilyName = originalPackageEntry.Id.FamilyName,
            Publisher = originalPackageEntry.Id.Publisher,
            ResourceId = originalPackageEntry.Id.ResourceId,
            Architecture = Enum.Parse<AppxPackageArchitecture>(originalPackageEntry.Id.Architecture.ToString(), true),
            IsFramework = originalPackageEntry.IsFramework,
            IsOptional = originalPackageEntry.IsOptional,
            TileColor = details.Color,
            PackageType = details.PackageType,
            Version = new Version(originalPackageEntry.Id.Version.Major, originalPackageEntry.Id.Version.Minor, originalPackageEntry.Id.Version.Build, originalPackageEntry.Id.Version.Revision),
            SignatureKind = ConvertSignatureKind(originalPackageEntry.SignatureKind),
            InstallDate = installDate,
            AppInstallerUri = originalPackageEntry.GetAppInstallerInfo()?.Uri
        };

        try
        {
            pkg.DisplayName = string.IsNullOrEmpty(originalPackageEntry.DisplayName) ? details.DisplayName : originalPackageEntry.DisplayName;
            pkg.DisplayPublisherName = string.IsNullOrEmpty(originalPackageEntry.PublisherDisplayName) ? details.DisplayPublisherName : originalPackageEntry.PublisherDisplayName;
            pkg.Description = string.IsNullOrEmpty(originalPackageEntry.Description) ? details.Description : originalPackageEntry.Description;
        }
        catch (COMException e)
        {
            if (e.HResult != -2147009780) // = 0x80073B0C
            {
                Logger.Warn().WriteLine(e, "Could not read details for package " + pkg.PackageFullName);
                return null;
            }

            // Workaround for exception
            // The ResourceMap or NamedResource has an item that does not have default or neutral 
            // resource. (Exception from HRESULT: 0x80073B0C)
            pkg.DisplayName = details.DisplayName;
            pkg.DisplayPublisherName = details.DisplayPublisherName;
            pkg.Description = details.Description;
        }

        if (installLocation != null && (pkg.DisplayName?.StartsWith("ms-resource:", StringComparison.Ordinal) ??
                                        pkg.DisplayPublisherName?.StartsWith("ms-resource:", StringComparison.Ordinal) ??
                                        pkg.Description?.StartsWith("ms-resource:", StringComparison.Ordinal) == true))
        {
            var priFile = Path.Combine(installLocation, "resources.pri");
            var resourceTranslator = new ResourceTranslator(originalPackageEntry.Id.FullName, priFile);

            pkg.DisplayName = resourceTranslator.Translate(pkg.DisplayName);
            pkg.DisplayPublisherName = resourceTranslator.Translate(pkg.DisplayPublisherName);
            pkg.Description = resourceTranslator.Translate(pkg.Description);

            if (string.IsNullOrEmpty(pkg.DisplayName))
            {
                pkg.DisplayName = pkg.Name;
            }

            if (string.IsNullOrEmpty(pkg.DisplayPublisherName))
            {
                pkg.DisplayPublisherName = pkg.Publisher;
            }
        }

        if (checkIfRunning)
        {
            var detector = new RunningAppsDetector();
            var getRunning = detector.GetCurrentlyRunningPackageFamilyNames();
            if (getRunning.Contains(pkg.PackageFamilyName))
            {
                pkg.IsRunning = true;
            }
        }

        return pkg;
    }

    private static async Task<AppxPackageQueryService.MsixPackageVisuals> GetVisualsFromManifest(string rootFolder, CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var reader = await AppxManifestSummaryReader.FromInstallLocation(rootFolder).ConfigureAwait(false);
            var logo = Path.Combine(rootFolder, reader.Logo);

            if (File.Exists(Path.Combine(rootFolder, logo)))
            {
                return new AppxPackageQueryService.MsixPackageVisuals(
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
            var dirInfo = new DirectoryInfo(Path.Combine(rootFolder, baseFolder));
            if (dirInfo.Exists)
            {
                var found = dirInfo.EnumerateFiles(baseName + "*" + extension).FirstOrDefault();
                if (found != null)
                {
                    logo = found.FullName;
                }
            }

            cancellationToken.ThrowIfCancellationRequested();
            return new AppxPackageQueryService.MsixPackageVisuals(reader.DisplayName, reader.DisplayPublisher, logo, reader.Description, reader.AccentColor, reader.PackageType);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            return new AppxPackageQueryService.MsixPackageVisuals();
        }
    }

    private static SignatureKind ConvertSignatureKind(PackageSignatureKind signatureKind)
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
}