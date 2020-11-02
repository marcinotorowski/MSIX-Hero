using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Packaging.Manifest;
using Otor.MsixHero.Appx.Packaging.Manifest.FileReaders;
using Otor.MsixHero.Appx.Updates.Entities;
using Otor.MsixHero.Appx.Updates.Serialization;
using Otor.MsixHero.Infrastructure.ThirdParty.Exceptions;
using Otor.MsixHero.Infrastructure.ThirdParty.Sdk;

namespace Otor.MsixHero.Appx.Updates
{
    public class MsixUpdateImpactAnalyzer : IAppxUpdateImpactAnalyzer
    {
        private readonly MsixSdkWrapper sdkWrapper;
        private readonly IAppxBlockReader blockReader;

        public MsixUpdateImpactAnalyzer(MsixSdkWrapper sdkWrapper, IAppxBlockReader blockReader)
        {
            this.sdkWrapper = sdkWrapper;
            this.blockReader = blockReader;
        }

        public async Task<UpdateImpactResult> Analyze(string msixPath1, string msixPath2, CancellationToken cancellationToken = default)
        {
            if (msixPath1 == null)
            {
                throw new ArgumentNullException(nameof(msixPath1));
            }

            if (msixPath2 == null)
            {
                throw new ArgumentNullException(nameof(msixPath2));
            }

            if (!IsMsixPackage(msixPath1))
            {
                throw new ArgumentException($"File {Path.GetFileName(msixPath1)} is not a valid MSIX.");
            }

            if (!IsMsixPackage(msixPath2))
            {
                throw new ArgumentException($"File {Path.GetFileName(msixPath2)} is not a valid MSIX.");
            }

            var tempFile = new FileInfo(Path.Join(Path.GetTempPath(), "msix-hero-cp-" + Guid.NewGuid().ToString("N").Substring(0, 8)));
            var result = new UpdateImpactResult();

            var manifestReader = new AppxManifestReader();
            using (var reader1 = new ZipArchiveFileReaderAdapter(msixPath1))
            {
                result.OldPackage = new UpdateImpactPackage();
                result.OldPackage.Blocks = await this.blockReader.ReadBlocks(reader1, cancellationToken).ConfigureAwait(false);
                result.OldPackage.Manifest = await manifestReader.Read(reader1, cancellationToken).ConfigureAwait(false);
            }

            using (var reader2 = new ZipArchiveFileReaderAdapter(msixPath2))
            {
                result.NewPackage = new UpdateImpactPackage();
                result.NewPackage.Blocks = await this.blockReader.ReadBlocks(reader2, cancellationToken).ConfigureAwait(false);
                result.NewPackage.Manifest = await manifestReader.Read(reader2, cancellationToken).ConfigureAwait(false);
            }

            result.OldPackage.Size = new FileInfo(msixPath1).Length;
            result.NewPackage.Size = new FileInfo(msixPath2).Length;

            try
            {
                await this.sdkWrapper.ComparePackages(msixPath1, msixPath2, tempFile.FullName, cancellationToken).ConfigureAwait(false);

                var serializer = new ComparePackageSerializer();
                result.Comparison = serializer.Deserialize(tempFile);

                this.blockReader.SetBlocks(result.OldPackage.Blocks, result.NewPackage.Blocks, result.Comparison);

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

        private static bool IsMsixPackage(string path)
        {
            try
            {
                using (IAppxFileReader fileReader = new ZipArchiveFileReaderAdapter(path))
                {
                    return fileReader.FileExists("AppxBlockMap.xml");
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}