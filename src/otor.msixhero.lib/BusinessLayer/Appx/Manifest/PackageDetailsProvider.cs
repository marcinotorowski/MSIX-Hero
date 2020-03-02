using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Appx.Manifest.FileReaders;
using otor.msixhero.lib.Domain.Appx.Manifest.Full;
using otor.msixhero.lib.Domain.Appx.Packages;
using otor.msixhero.lib.Infrastructure.Logging;
using otor.msixhero.lib.Infrastructure.Progress;

namespace otor.msixhero.lib.BusinessLayer.Appx.Manifest
{
    public class PackageDetailsProvider
    {
        private static readonly ILog Logger = LogManager.GetLogger();

        public async Task<AppxPackage> GetPackageByManifestFilePath(
            string fullPath,
            PackageFindMode mode = PackageFindMode.CurrentUser,
            CancellationToken cancellationToken = default,
            IProgress<ProgressData> progress = default)
        {
            using IAppxFileReader fileReader = new FileInfoFileReaderAdapter(fullPath);
            return await GetPackage(fileReader, mode, cancellationToken, progress).ConfigureAwait(false);
        }

        public async Task<AppxPackage> GetPackageByIdentity(
            string packageName,
            PackageFindMode mode = PackageFindMode.CurrentUser, 
            CancellationToken cancellationToken = default,
            IProgress<ProgressData> progress = default)
        {
            using var reader = new PackageIdentityFileReaderAdapter(mode == PackageFindMode.CurrentUser ? PackageContext.CurrentUser : PackageContext.AllUsers, packageName);
            return await GetPackage(reader, mode, cancellationToken, progress).ConfigureAwait(false);
        }

        private static async Task<AppxPackage> GetPackage(
            IAppxFileReader fileReader,
            PackageFindMode mode = PackageFindMode.CurrentUser,
            CancellationToken cancellationToken = default,
            IProgress<ProgressData> progress = default)
        {
            progress?.Report(new ProgressData(10, "Reading package..."));

            var manifestReader = new AppxManifestReader();
            var package = await Task.Run(() => manifestReader.Read(fileReader), cancellationToken).ConfigureAwait(false);
            if (!package.PackageDependencies.Any())
            {
                progress?.Report(new ProgressData(100, "Reading package..."));
                return package;
            }

            var single = 90.0 / package.PackageDependencies.Count;
            var total = 10.0;

            foreach (var depPackage in package.PackageDependencies)
            {
                cancellationToken.ThrowIfCancellationRequested();
                total += single;
                progress?.Report(new ProgressData((int) total, "Reading dependencies..."));

                try
                {
                    using var reader = new PackageIdentityFileReaderAdapter(mode == PackageFindMode.CurrentUser ? PackageContext.CurrentUser : PackageContext.AllUsers, depPackage.Name, depPackage.Publisher);
                    // ReSharper disable once AccessToDisposedClosure
                    depPackage.Dependency = await Task.Run(() => manifestReader.Read(reader), cancellationToken).ConfigureAwait(false);
                }
                catch (FileNotFoundException)
                {
                    Logger.Warn("Could not find dependency package {0} by {1}.", depPackage.Name, depPackage.Publisher);
                }
            }

            return package;
        }
    }
}
