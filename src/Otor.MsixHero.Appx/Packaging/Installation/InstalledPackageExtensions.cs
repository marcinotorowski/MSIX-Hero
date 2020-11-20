using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;
using Otor.MsixHero.Appx.Packaging.Installation.Enums;
using Otor.MsixHero.Appx.Packaging.Manifest;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;
using Otor.MsixHero.Appx.Packaging.Manifest.FileReaders;

namespace Otor.MsixHero.Appx.Packaging.Installation
{
    public static class InstalledPackageExtensions
    {
        public static async Task<AppxPackage> ToAppxPackage(this InstalledPackage pkg, CancellationToken cancellationToken = default)
        {
            if (pkg.InstallLocation == null)
            {
                return null;
            }

            var manifestReader = new AppxManifestReader();

            IAppxFileReader reader;
            if (pkg.ManifestLocation == null || !File.Exists(pkg.ManifestLocation))
            {
                reader = new PackageIdentityFileReaderAdapter(PackageContext.CurrentUser, pkg.PackageId);
            }
            else
            {
                reader = new FileInfoFileReaderAdapter(pkg.ManifestLocation);
            }

            using (reader)
            {
                return await manifestReader.Read(reader, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
