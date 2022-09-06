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
            uint? sizeInMegaBytes,
            CancellationToken cancellationToken = default,
            IProgress<ProgressData> progressReporter = null)
        {
            if (packagePath == null)
            {
                throw new ArgumentNullException(nameof(packagePath), Resources.Localization.Packages_Error_EmptyPath);
            }

            if (volumePath == null)
            {
                throw new ArgumentNullException(nameof(volumePath), Resources.Localization.Packages_Error_EmptyVolumePath);
            }

            Logger.Debug().WriteLine("Unpacking {0} with MSIXMGR…", packagePath);
            progressReporter?.Report(new ProgressData(20, string.Format(Resources.Localization.Packages_AppAttach_Unpacking_Format, Path.GetFileName(packagePath))));

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
                    throw new NotSupportedException(string.Format(Resources.Localization.Packages_Error_DiskFormatNotSupported, Path.GetExtension(volumePath)));
            }

            uint size;
            if (sizeInMegaBytes.HasValue && sizeInMegaBytes.Value > 0)
            {
                size = sizeInMegaBytes.Value;
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
                        throw new NotSupportedException(string.Format(Resources.Localization.Packages_Error_ExtensionNotSupported_Format, Path.GetExtension(volumePath)));
                }

                size = await sizeCalculator.GetRequiredSize(packagePath, cancellationToken: cancellationToken).ConfigureAwait(false);
            }
            
            // These value are taken from what msixmgr seems to report:
            if (size < 5)
            {
                size = 5;
            }
            else if (size > 2040000)
            {
                size = 2040000;
            }

            Logger.Info().WriteLine("Expanding MSIX…");
            await this.MsixMgr.UnpackEx(
                packagePath,
                volumePath,
                fileType,
                size,
                true,
                true,
                Path.GetFileNameWithoutExtension(packagePath),
                cancellationToken).ConfigureAwait(false);
        }
    }
}
