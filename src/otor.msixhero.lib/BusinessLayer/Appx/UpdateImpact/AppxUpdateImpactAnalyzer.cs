using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Appx.Manifest;
using otor.msixhero.lib.BusinessLayer.Appx.Manifest.FileReaders;
using otor.msixhero.lib.Domain.Appx.UpdateImpact;
using otor.msixhero.lib.Infrastructure.Wrappers;

namespace otor.msixhero.lib.BusinessLayer.Appx.UpdateImpact
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

                    case PackageType.AppxBlockMap:
                    {
                        var comparer = new AppxBlockMapUpdateImpactAnalyzer(new MsixSdkWrapper(), this.BlockReader);
                        return await comparer.Analyze(package1Path, package2Path, cancellationToken).ConfigureAwait(false);
                    }
                }
            }
            else
            {
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
                case ".msixbundle":
                case ".appxbundle":
                    return PackageType.Msix;

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
            Unsupported
        }
    }
}
