using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Appx.UpdateImpact.Serialization;
using otor.msixhero.lib.Domain.Appx.UpdateImpact;
using otor.msixhero.lib.Infrastructure.Wrappers;
using File = System.IO.File;

namespace otor.msixhero.lib.BusinessLayer.Appx.UpdateImpact
{
    public class AppxBlockMapUpdateImpactAnalyzer : IAppxUpdateImpactAnalyzer
    {
        private readonly MsixSdkWrapper sdkWrapper;
        private readonly IAppxBlockReader blockReader;

        public AppxBlockMapUpdateImpactAnalyzer(MsixSdkWrapper sdkWrapper, IAppxBlockReader blockReader)
        {
            this.sdkWrapper = sdkWrapper;
            this.blockReader = blockReader;
        }

        public async Task<UpdateImpactResult> Analyze(string appxBlockMapPath1, string appxBlockMapPath2, CancellationToken cancellationToken = default)
        {
            var file1Name = Path.GetFileName(appxBlockMapPath1);
            var file2Name = Path.GetFileName(appxBlockMapPath2);

            if (!string.Equals(file1Name, "AppxBlockMap.xml", StringComparison.OrdinalIgnoreCase) || !string.Equals(file2Name, "AppxBlockMap.xml", StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException("This analyzer can only compare two appx block maps.");
            }

            var tempFile = Path.Join(Path.GetTempPath(), "msix-hero-cp-" + Guid.NewGuid().ToString("N").Substring(0, 8));
            var results = new UpdateImpactResult();
            try
            {
                await this.sdkWrapper.ComparePackages(appxBlockMapPath1, appxBlockMapPath2, tempFile, cancellationToken).ConfigureAwait(false);
                var serializer = new ComparePackageSerializer();
                results.Comparison = serializer.Deserialize(tempFile);
                results.OldPackage = new UpdateImpactPackage { Path = appxBlockMapPath1 };
                results.NewPackage = new UpdateImpactPackage { Path = appxBlockMapPath2 };

                results.OldPackage.Blocks = await this.blockReader.ReadBlocks(new FileInfo(appxBlockMapPath1), cancellationToken).ConfigureAwait(false);
                results.NewPackage.Blocks = await this.blockReader.ReadBlocks(new FileInfo(appxBlockMapPath2), cancellationToken).ConfigureAwait(false);
                this.blockReader.SetBlocks(results.OldPackage.Blocks, results.NewPackage.Blocks, results.Comparison);

                return results;
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }
    }
}