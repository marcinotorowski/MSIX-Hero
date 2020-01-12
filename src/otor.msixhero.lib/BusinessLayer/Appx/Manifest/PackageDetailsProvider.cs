using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Management.Deployment;
using otor.msixhero.lib.BusinessLayer.Appx.Manifest.FileReaders;
using otor.msixhero.lib.Domain.Appx.Manifest.Full;
using otor.msixhero.lib.Domain.Appx.Packages;
using otor.msixhero.lib.Infrastructure.Logging;
using otor.msixhero.lib.Infrastructure.Progress;
using Package = Windows.ApplicationModel.Package;

namespace otor.msixhero.lib.BusinessLayer.Appx.Manifest
{
    public class PackageDetailsProvider
    {
        private static readonly ILog Logger = LogManager.GetLogger();

        public PackageDetailsProvider(IAppxPackageManager packageManager)
        {
        }

        public async Task<AppxPackage> GetPackage(string packageName,
            PackageFindMode mode = PackageFindMode.CurrentUser, 
            CancellationToken cancellationToken = default,
            IProgress<ProgressData> progress = default)
        {
            var result = await Task.Run(() =>
            {
                var fuk = new AppxManifestReader();

                AppxPackage package;
                using (var reader = new PackageIdentityFileReaderAdapter(mode == PackageFindMode.CurrentUser ? PackageContext.CurrentUser : PackageContext.AllUsers, packageName))
                {
                    package = fuk.Read(reader);
                }

                if (!package.PackageDependencies.Any())
                {
                    return package;
                }

                foreach (var depPackage in package.PackageDependencies)
                {
                    try
                    {
                        using var reader = new PackageIdentityFileReaderAdapter(mode == PackageFindMode.CurrentUser ? PackageContext.CurrentUser : PackageContext.AllUsers,  depPackage.Name, depPackage.Publisher);
                        depPackage.Dependency = fuk.Read(reader);
                    }
                    catch (FileNotFoundException)
                    {
                        Logger.Warn("Could not find dependency package {0} by {1}.", depPackage.Name, depPackage.Publisher);
                    }
                }

                return package;
            }, cancellationToken).ConfigureAwait(false);
            
            var pkgMan = new PackageManager();
            Package managedPackage;

            switch (mode)
            {
                case PackageFindMode.CurrentUser:
                    managedPackage = pkgMan.FindPackageForUser(string.Empty, packageName);
                    break;
                case PackageFindMode.AllUsers:
                    managedPackage = pkgMan.FindPackage(packageName);
                    break;
                default:
                    throw new NotSupportedException();
            }

            result.Addons = new List<AppxPackage>();

            if (managedPackage != null)
            {
                foreach (var managedDependency in managedPackage.Dependencies.Where(p => p.IsOptional))
                {
                    if (result.PackageDependencies.Any(mpd => mpd.Dependency.FullName == managedDependency.Id.FullName))
                    {
                        continue;
                    }

                    var dependency = await this.GetPackage(managedDependency.Id.FullName, mode, cancellationToken, progress).ConfigureAwait(false);
                    if (dependency == null)
                    {
                        continue;
                    }

                    result.Addons.Add(dependency);
                }
            }

            return result;
        }
    }
}
