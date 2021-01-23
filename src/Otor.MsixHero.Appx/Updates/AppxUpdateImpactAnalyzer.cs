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
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Packaging.Manifest;
using Otor.MsixHero.Appx.Packaging.Manifest.FileReaders;
using Otor.MsixHero.Appx.Updates.Entities;
using Otor.MsixHero.Infrastructure.ThirdParty.Sdk;

namespace Otor.MsixHero.Appx.Updates
{
    public class AppxUpdateImpactAnalyzer : IAppxUpdateImpactAnalyzer
    {
        protected readonly IAppxBlockReader BlockReader = new AppxBlockReader();
        
        public async Task<UpdateImpactResult> Analyze(string package1Path, string package2Path, CancellationToken cancellationToken = default)
        {
            var type1 = GetPackageTypeFromFile(package1Path);
            if (type1 == PackageType.Unsupported)
            {
                throw new ArgumentException($"File {Path.GetFileName(package1Path)} is not supported.");
            }

            var type2 = GetPackageTypeFromFile(package2Path);
            if (type2 == PackageType.Unsupported)
            {
                throw new ArgumentException($"File {Path.GetFileName(package2Path)} is not supported.");
            }

            if (type1 == type2)
            {
                switch (type1)
                {
                    case PackageType.Msix:
                    {
                        var comparer = new MsixUpdateImpactAnalyzer(new MsixSdkWrapper(), this.BlockReader);
                        return await comparer.Analyze(package1Path, package2Path, cancellationToken).ConfigureAwait(false);
                    }

                    case PackageType.Manifest:
                    {
                        var comparer = new AppxBlockMapUpdateImpactAnalyzer(new MsixSdkWrapper(), this.BlockReader);
                        var appxBlock1 = Path.Join(Path.GetDirectoryName(package1Path), "AppxBlockMap.xml");
                        var appxBlock2 = Path.Join(Path.GetDirectoryName(package2Path), "AppxBlockMap.xml");
                        return await comparer.Analyze(appxBlock1, appxBlock2, cancellationToken).ConfigureAwait(false);
                    }

                    case PackageType.Bundle:
                    {
                        var comparer = new BundleUpdateImpactAnalyzer();
                        return await comparer.Analyze(package1Path, package2Path, cancellationToken).ConfigureAwait(false);
                        }

                    case PackageType.AppxBlockMap:
                    {
                        var comparer = new AppxBlockMapUpdateImpactAnalyzer(new MsixSdkWrapper(), this.BlockReader);
                        return await comparer.Analyze(package1Path, package2Path, cancellationToken).ConfigureAwait(false);
                    }
                }
            }
            else
            {
                if (type2 == PackageType.Bundle || type1 == PackageType.Bundle)
                {
                    throw new NotSupportedException("A bundle can be only compared to another bundle.");
                }

                string appxBlock1 = null;
                string appxBlock2 = null;

                var tempPaths = new List<string>();
                var results = new UpdateImpactResult();

                var manifestReader = new AppxManifestReader();
                try
                {
                    switch (type1)
                    {
                        case PackageType.Msix:
                            using (IAppxFileReader fileReader = new ZipArchiveFileReaderAdapter(package1Path))
                            {
                                using (var msixStream = fileReader.GetFile("AppxBlockMap.xml"))
                                {
                                    var tempFile = Path.Join(Path.GetTempPath(), "msix-hero-cp-" + Guid.NewGuid().ToString("N").Substring(0, 8));
                                    using (var fs = File.OpenWrite(tempFile))
                                    {
                                        await msixStream.CopyToAsync(fs, cancellationToken).ConfigureAwait(false);
                                        await fs.FlushAsync(cancellationToken).ConfigureAwait(false);
                                    }

                                    tempPaths.Add(tempFile);
                                    appxBlock1 = tempFile;

                                    results.OldPackage = new UpdateImpactPackage
                                    {
                                        Size = new FileInfo(package1Path).Length,
                                        Path = package1Path,
                                        Manifest = await manifestReader.Read(fileReader, cancellationToken).ConfigureAwait(false)
                                    };
                                }
                            }

                            break;
                        case PackageType.Manifest:
                            appxBlock1 = Path.Join(Path.GetDirectoryName(package1Path), "AppxBlockMap.xml");

                            using (IAppxFileReader fileReader = new FileInfoFileReaderAdapter(package1Path))
                            {
                                results.NewPackage = new UpdateImpactPackage
                                {
                                    Path = package1Path,
                                    Manifest = await manifestReader.Read(fileReader, cancellationToken).ConfigureAwait(false)
                                };
                            }

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
                                using (var msixStream = fileReader.GetFile("AppxBlockMap.xml"))
                                {
                                    var tempFile = Path.Join(Path.GetTempPath(), "msix-hero-cp-" + Guid.NewGuid().ToString("N").Substring(0, 8));
                                    using (var fs = File.OpenWrite(tempFile))
                                    {
                                        await msixStream.CopyToAsync(fs, cancellationToken).ConfigureAwait(false);
                                        await fs.FlushAsync(cancellationToken).ConfigureAwait(false);
                                    }

                                    tempPaths.Add(tempFile);
                                    appxBlock2 = tempFile;

                                    results.NewPackage = new UpdateImpactPackage
                                    {
                                        Size = new FileInfo(package2Path).Length,
                                        Path = package2Path,
                                        Manifest = await manifestReader.Read(fileReader, cancellationToken).ConfigureAwait(false)
                                    };
                                }
                            }

                            break;
                        case PackageType.Manifest:
                            appxBlock2 = Path.Join(Path.GetDirectoryName(package2Path), "AppxBlockMap.xml");

                            using (IAppxFileReader fileReader = new FileInfoFileReaderAdapter(package2Path))
                            {
                                results.NewPackage = new UpdateImpactPackage
                                {
                                    Path = package2Path,
                                    Manifest = await manifestReader.Read(fileReader, cancellationToken).ConfigureAwait(false)
                                };
                            }

                            break;
                        case PackageType.AppxBlockMap:
                            appxBlock2 = package2Path;
                            break;
                    }

                    var comparer = new AppxBlockMapUpdateImpactAnalyzer(new MsixSdkWrapper(), this.BlockReader);
                    var comparedResults = await comparer.Analyze(appxBlock1, appxBlock2, cancellationToken).ConfigureAwait(false);
                    results.Comparison = comparedResults.Comparison;

                    if (results.OldPackage == null)
                    {
                        results.OldPackage = comparedResults.OldPackage;
                    }

                    if (results.NewPackage == null)
                    {
                        results.OldPackage = comparedResults.NewPackage;
                    }

                    return results;
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
        
        private static PackageType GetPackageTypeFromFile(string filePath)
        {
            switch (Path.GetExtension(filePath).ToLowerInvariant())
            {
                case ".msix":
                case ".appx":
                    return PackageType.Msix;
                case ".msixbundle":
                case ".appxbundle":
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
