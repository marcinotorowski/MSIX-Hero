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
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Appx.Packaging.Manifest;
using Otor.MsixHero.Appx.Packaging.Manifest.FileReaders;
using Otor.MsixHero.Appx.Updates.Entities;
using Otor.MsixHero.Infrastructure.Progress;

namespace Otor.MsixHero.Appx.Updates
{
    public class AppxUpdateImpactAnalyzer : IAppxUpdateImpactAnalyzer
    {
        public async Task<UpdateImpactResults> Analyze(
            string package1Path,
            string package2Path,
            bool ignoreVersionCheck = false,
            CancellationToken cancellationToken = default,
            IProgress<ProgressData> progressReporter = default)
        {
            var type1 = GetPackageTypeFromFile(package1Path);
            if (type1 == PackageType.Unsupported)
            {
                throw new ArgumentException(string.Format(Resources.Localization.Packages_UpdateImpact_Error_FileNotsupported_Format, Path.GetFileName(package1Path)));
            }

            var type2 = GetPackageTypeFromFile(package2Path);
            if (type2 == PackageType.Unsupported)
            {
                throw new ArgumentException(string.Format(Resources.Localization.Packages_UpdateImpact_Error_FileNotsupported_Format, Path.GetFileName(package2Path)));
            }

            await AssertPackagesUpgradable(package1Path, package2Path, ignoreVersionCheck).ConfigureAwait(false);
            
            if (type1 == type2)
            {
                switch (type1)
                {
                    case PackageType.Msix:
                    {
                        var comparer = new MsixUpdateImpactAnalyzer();
                        var result = await comparer.Analyze(package1Path, package2Path, ignoreVersionCheck, cancellationToken, progressReporter).ConfigureAwait(false);
                        result.OldPackage = package1Path;
                        result.NewPackage = package2Path;
                        return result;
                    }

                    case PackageType.Manifest:
                    {
                        var comparer = new AppxBlockMapUpdateImpactAnalyzer();
                        var appxBlock1 = Path.Join(Path.GetDirectoryName(package1Path), "AppxBlockMap.xml");
                        var appxBlock2 = Path.Join(Path.GetDirectoryName(package2Path), "AppxBlockMap.xml");
                        var result = await comparer.Analyze(appxBlock1, appxBlock2, ignoreVersionCheck, cancellationToken, progressReporter).ConfigureAwait(false);
                        result.OldPackage = package1Path;
                        result.NewPackage = package2Path;
                        return result;
                    }

                    case PackageType.Bundle:
                    {
                        var comparer = new BundleUpdateImpactAnalyzer();
                        var result =await comparer.Analyze(package1Path, package2Path, ignoreVersionCheck, cancellationToken, progressReporter).ConfigureAwait(false);
                        result.OldPackage = package1Path;
                        result.NewPackage = package2Path;
                        return result;
                    }

                    case PackageType.AppxBlockMap:
                    {
                        var comparer = new AppxBlockMapUpdateImpactAnalyzer();
                        var result = await comparer.Analyze(package1Path, package2Path, ignoreVersionCheck, cancellationToken, progressReporter).ConfigureAwait(false);
                        result.OldPackage = package1Path;
                        result.NewPackage = package2Path;
                        return result;
                    }
                }
            }
            else
            {
                if (type2 == PackageType.Bundle || type1 == PackageType.Bundle)
                {
                    throw new NotSupportedException(Resources.Localization.Packages_UpdateImpact_Error_BundleMismatch);
                }

                string appxBlock1 = null;
                string appxBlock2 = null;

                var tempPaths = new List<string>();
                
                try
                {
                    switch (type1)
                    {
                        case PackageType.Msix:
                            using (IAppxFileReader fileReader = new ZipArchiveFileReaderAdapter(package1Path))
                            {
                                await using var msixStream = fileReader.GetFile("AppxBlockMap.xml");
                                var tempFile = Path.Join(Path.GetTempPath(), "msix-hero-cp-" + Guid.NewGuid().ToString("N").Substring(0, 8));
                                await using (var fs = File.OpenWrite(tempFile))
                                {
                                    await msixStream.CopyToAsync(fs, cancellationToken).ConfigureAwait(false);
                                    await fs.FlushAsync(cancellationToken).ConfigureAwait(false);
                                }

                                tempPaths.Add(tempFile);
                                appxBlock1 = tempFile;
                            }

                            break;
                        case PackageType.Manifest:
                            appxBlock1 = Path.Join(Path.GetDirectoryName(package1Path), "AppxBlockMap.xml");
                            break;
                        case PackageType.AppxBlockMap:
                            appxBlock1 = package1Path;
                            break;
                    }

                    switch (type2)
                    {
                        case PackageType.Msix:
                            using (IAppxFileReader fileReader = new ZipArchiveFileReaderAdapter(package2Path))
                            {
                                await using var msixStream = fileReader.GetFile("AppxBlockMap.xml");
                                var tempFile = Path.Join(Path.GetTempPath(), "msix-hero-cp-" + Guid.NewGuid().ToString("N").Substring(0, 8));
                                await using (var fs = File.OpenWrite(tempFile))
                                {
                                    await msixStream.CopyToAsync(fs, cancellationToken).ConfigureAwait(false);
                                    await fs.FlushAsync(cancellationToken).ConfigureAwait(false);
                                }

                                tempPaths.Add(tempFile);
                                appxBlock2 = tempFile;
                            }

                            break;
                        case PackageType.Manifest:
                            appxBlock2 = Path.Join(Path.GetDirectoryName(package2Path), "AppxBlockMap.xml");
                            break;
                        case PackageType.AppxBlockMap:
                            appxBlock2 = package2Path;
                            break;
                    }

                    var comparer = new AppxBlockMapUpdateImpactAnalyzer();
                    var result = await comparer.Analyze(appxBlock1, appxBlock2, ignoreVersionCheck, cancellationToken, progressReporter).ConfigureAwait(false);
                    result.OldPackage = package1Path;
                    result.NewPackage = package2Path;
                    return result;
                }
                finally
                {
                    foreach (var item in tempPaths.Where(File.Exists))
                    {
                        File.Delete(item);
                    }
                }
            }

            throw new NotSupportedException();
        }

        private static async Task AssertPackagesUpgradable(string package1Path, string package2Path, bool ignoreVersionCheck = false)
        {
            if (string.IsNullOrEmpty(package1Path))
            {
                throw new ArgumentNullException(nameof(package1Path));
            }

            if (string.IsNullOrEmpty(package2Path))
            {
                throw new ArgumentNullException(nameof(package2Path));
            }
            
            var manifestReader = new AppxManifestReader();
            string packageFamily1, packageFamily2, version1, version2, name1, name2;

            try
            {
                using var fileReader1 = FileReaderFactory.CreateFileReader(package1Path);
                var file1 = await manifestReader.Read(fileReader1).ConfigureAwait(false);
                packageFamily1 = file1.FamilyName;
                version1 = file1.Version;
                name1 = file1.DisplayName;
            }
            catch (Exception e)
            {
                throw new UpdateImpactException(string.Format(Resources.Localization.Packages_UpdateImpact_Error_FormatNotSupported_Format, package1Path), UpgradeImpactError.WrongPackageFormat, e);
            }
            
            try
            {
                using var fileReader2 = FileReaderFactory.CreateFileReader(package2Path);
                var file2 = await manifestReader.Read(fileReader2).ConfigureAwait(false);
                packageFamily2 = file2.FamilyName;
                version2 = file2.Version;
                name2 = file2.DisplayName;
            }
            catch (Exception e)
            {
                throw new UpdateImpactException(string.Format(Resources.Localization.Packages_UpdateImpact_Error_FormatNotSupported_Format, package2Path), UpgradeImpactError.WrongPackageFormat, e);
            }

            if (!string.Equals(packageFamily1, packageFamily2))
            {
                throw new UpdateImpactException(string.Format(Resources.Localization.Packages_UpdateImpact_Error_SameFamily_Format, name2, name1), UpgradeImpactError.WrongFamilyName);
            }

            if (ignoreVersionCheck)
            {
                return;
            }
            
            if (Version.Parse(version2) <= Version.Parse(version1))
            {
                throw new UpdateImpactException(string.Format(Resources.Localization.Packages_UpdateImpact_Error_Version_Format, name2, version2, name1, version1), UpgradeImpactError.WrongFamilyName);
            }
        }

        private static PackageType GetPackageTypeFromFile(string filePath)
        {
            switch (Path.GetExtension(filePath).ToLowerInvariant())
            {
                case FileConstants.MsixExtension:
                case FileConstants.AppxExtension:
                    return PackageType.Msix;
                case FileConstants.MsixBundleExtension:
                case FileConstants.AppxBundleExtension:
                    return PackageType.Bundle;

                case ".xml":
                    switch (Path.GetFileNameWithoutExtension(filePath).ToLowerInvariant())
                    {
                        case "appxmanifest":
                            return PackageType.Manifest;
                        case "appxblockmap":
                            return PackageType.AppxBlockMap;
                    }

                    break;
            }

            return PackageType.Unsupported;
        }

        private enum PackageType
        {
            Msix,
            Manifest,
            AppxBlockMap,
            Bundle,
            Unsupported
        }
    }
}
