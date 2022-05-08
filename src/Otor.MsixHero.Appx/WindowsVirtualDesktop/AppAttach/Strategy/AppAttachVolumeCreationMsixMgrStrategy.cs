using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Appx.WindowsVirtualDesktop.AppAttach.SizeCalculator;
using Dapplo.Log;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.ThirdParty.Sdk;

namespace Otor.MsixHero.Appx.WindowsVirtualDesktop.AppAttach.Strategy
{
    public class AppAttachVolumeCreationMsixMgrStrategy : IAppAttachVolumeCreationStrategy
    {
        private static readonly LogSource Logger = new();
        protected readonly MsixMgrWrapper MsixMgr = new MsixMgrWrapper();

        public Task<IAppAttachVolumeCreationStrategyInitialization> Initialize(CancellationToken cancellation = default)
        {
            return Task.FromResult(default(IAppAttachVolumeCreationStrategyInitialization));
        }

        public Task Finish(IAppAttachVolumeCreationStrategyInitialization data, CancellationToken cancellation = default)
        {
            return Task.CompletedTask;
        }

        public async Task CreateVolume(
            string packagePath,
            string volumePath,
            long? customSizeInBytes,
            CancellationToken cancellationToken = default,
            IProgress<ProgressData> progressReporter = null)
        {
            if (volumePath == null)
            {
                throw new ArgumentNullException(nameof(volumePath), "Volume path must be empty.");
            }

            if (packagePath == null)
            {
                throw new ArgumentNullException(nameof(packagePath), "Volume path must be empty.");
            }

            Logger.Debug().WriteLine("Unpacking {0} with MSIXMGR...", packagePath);
            progressReporter?.Report(new ProgressData(20, $"Unpacking {Path.GetFileName(packagePath)}..."));

            MsixMgrWrapper.FileType fileType;

            switch (Path.GetExtension(volumePath).ToLowerInvariant())
            {
                case FileConstants.AppAttachVhdxExtension:
                    fileType = MsixMgrWrapper.FileType.Vhdx;
                    break;
                case FileConstants.AppAttachVhdExtension:
                    fileType = MsixMgrWrapper.FileType.Vhd;
                    break;
                case FileConstants.AppAttachCimExtension:
                    fileType = MsixMgrWrapper.FileType.Cim;
                    break;
                default:
                    throw new NotSupportedException($"Disk format {Path.GetExtension(volumePath)} is not supported.");
            }

            long size;
            if (customSizeInBytes.HasValue && customSizeInBytes.Value > 0)
            {
                size = customSizeInBytes.Value;
            }
            else
            {
                ISizeCalculator sizeCalculator;

                switch (Path.GetExtension(volumePath).ToLowerInvariant())
                {
                    case FileConstants.AppAttachVhdExtension:
                    case FileConstants.AppAttachVhdxExtension:
                        sizeCalculator = new VhdSizeCalculator();
                        break;
                    case FileConstants.AppAttachCimExtension:
                        sizeCalculator = new CimSizeCalculator();
                        break;
                    default:
                        throw new NotSupportedException("Extension " + Path.GetExtension(volumePath) + " is not supported.");
                }

                size = await sizeCalculator.GetRequiredSize(packagePath, cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            if (fileType == MsixMgrWrapper.FileType.Cim)
            {
                size = (long)Math.Max(5, size / (1024.0 * 1024));
            }

            Logger.Info().WriteLine("Expanding MSIX...");
            await this.MsixMgr.UnpackEx(
                packagePath,
                volumePath,
                fileType,
                (uint)size,
                true,
                true,
                Path.GetFileNameWithoutExtension(packagePath),
                cancellationToken).ConfigureAwait(false);
        }
    }
}
