using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Appx.Detection;
using otor.msixhero.lib.BusinessLayer.Helpers;
using otor.msixhero.lib.Domain.Appx.Manifest.Full;
using otor.msixhero.lib.Domain.Appx.Manifest.Summary;
using otor.msixhero.lib.Infrastructure.Progress;

namespace otor.msixhero.lib.BusinessLayer.Appx.Details
{
    public class PackageDetailsProvider
    {
        public Task<AppxPackage> GetPackage(string packageName, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            return Task.Run(() =>
            {
                var nativePkg = Native.QueryPackageInfo(packageName, Native.PackageConstants.PACKAGE_INFORMATION_FULL);

                AppxPackage mainApp = null;
                IList<AppxPackage> dependencies = new List<AppxPackage>();

                foreach (var item in nativePkg)
                {
                    if (mainApp == null)
                    {
                        mainApp = item;
                    }
                    else
                    {
                        dependencies.Add(item);
                    }
                }

                if (mainApp == null)
                {
                    return null;
                }

                foreach (var dependency in mainApp.PackageDependencies)
                {
                    dependency.Dependency = dependencies.FirstOrDefault(d =>
                        d.Publisher == dependency.Publisher &&
                        d.Name == dependency.Name &&
                        Version.Parse(d.Version) >= Version.Parse(dependency.Version));
                }

                return mainApp;
            }, cancellationToken);
        }
    }
}
