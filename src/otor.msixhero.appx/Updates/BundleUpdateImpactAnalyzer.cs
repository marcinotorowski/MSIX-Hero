using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Packaging.Manifest.FileReaders;
using Otor.MsixHero.Appx.Updates.Entities;
using Otor.MsixHero.Appx.Updates.Serialization;
using Otor.MsixHero.Infrastructure.ThirdParty.Exceptions;
using Otor.MsixHero.Infrastructure.ThirdParty.Sdk;

namespace Otor.MsixHero.Appx.Updates
{
    public class BundleUpdateImpactAnalyzer : IAppxUpdateImpactAnalyzer
    {
        private readonly MsixSdkWrapper sdkWrapper;

        public BundleUpdateImpactAnalyzer(MsixSdkWrapper sdkWrapper)
        {
            this.sdkWrapper = sdkWrapper;
        }

        public async Task<UpdateImpactResult> Analyze(string msixBundlePath1, string msixBundlePath2, CancellationToken cancellationToken = default)
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

            var tempFile = new FileInfo(Path.Join(Path.GetTempPath(), "msix-hero-cp-" + Guid.NewGuid().ToString("N").Substring(0, 8)));
            var result = new UpdateImpactResult();
            
            result.NewPackage = new UpdateImpactPackage();
            result.OldPackage = new UpdateImpactPackage();
            result.OldPackage.Size = new FileInfo(msixBundlePath1).Length;
            result.NewPackage.Size = new FileInfo(msixBundlePath2).Length;

            try
            {
                await this.sdkWrapper.ComparePackages(msixBundlePath1, msixBundlePath2, tempFile.FullName, cancellationToken).ConfigureAwait(false);

                var serializer = new ComparePackageSerializer();
                result.Comparison = serializer.Deserialize(tempFile);

                return result;
            }
            catch (ProcessWrapperException e)
            {
                throw new InvalidOperationException(string.Join(Environment.NewLine, e.StandardError.Where(s => !string.IsNullOrWhiteSpace(s))));
            }
            finally
            {
                if (tempFile.Exists)
                {
                    tempFile.Delete();
                }
            }
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