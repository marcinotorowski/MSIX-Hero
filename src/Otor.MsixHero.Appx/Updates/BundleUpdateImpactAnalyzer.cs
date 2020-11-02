using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Packaging.Manifest.FileReaders;
using Otor.MsixHero.Appx.Updates.Entities;

namespace Otor.MsixHero.Appx.Updates
{
    public class BundleUpdateImpactAnalyzer : IAppxUpdateImpactAnalyzer
    {
        public Task<UpdateImpactResult> Analyze(string msixBundlePath1, string msixBundlePath2, CancellationToken cancellationToken = default)
        {
            if (msixBundlePath1 == null)
            {
                throw new ArgumentNullException(nameof(msixBundlePath1));
            }

            if (msixBundlePath2 == null)
            {
                throw new ArgumentNullException(nameof(msixBundlePath2));
            }

            if (!IsBundlePackage(msixBundlePath1))
            {
                throw new ArgumentException($"File {Path.GetFileName(msixBundlePath1)} is not a valid MSIX bundle.");
            }

            if (!IsBundlePackage(msixBundlePath2))
            {
                throw new ArgumentException($"File {Path.GetFileName(msixBundlePath2)} is not a valid MSIX bundle.");
            }

            throw new NotSupportedException("Bundles are not supported.");
        }

        private static bool IsBundlePackage(string path)
        {
            try
            {
                using (IAppxFileReader fileReader = new ZipArchiveFileReaderAdapter(path))
                {
                    return fileReader.FileExists("AppxMetadata\\AppxBundleManifest.xml");
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}