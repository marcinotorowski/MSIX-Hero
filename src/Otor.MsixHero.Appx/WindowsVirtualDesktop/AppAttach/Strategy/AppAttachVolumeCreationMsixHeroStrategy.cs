// MSIX Hero
// Copyright (C) 2024 Marcin Otorowski
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// Full notice:
// https://github.com/marcinotorowski/msix-hero/blob/develop/LICENSE.md

using System;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Appx.WindowsVirtualDesktop.AppAttach.MountVol;
using Otor.MsixHero.Appx.WindowsVirtualDesktop.AppAttach.SizeCalculator;
using Dapplo.Log;
using Otor.MsixHero.Infrastructure.Helpers;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.ThirdParty.Sdk;

namespace Otor.MsixHero.Appx.WindowsVirtualDesktop.AppAttach.Strategy
{
    /// <summary>
    /// Alternate implementation of volume creation.
    /// This is a MSIX Hero 1.x way of doing things, but it seems it works better then msixmgr.exe which has its own quirks.
    /// </summary>
    public class AppAttachVolumeCreationMsixHeroStrategy : IAppAttachVolumeCreationStrategy
    {
        private static readonly LogSource Logger = new();        
        protected readonly MsixMgrWrapper MsixMgr = new MsixMgrWrapper(true);


        public async Task<IAppAttachVolumeCreationStrategyInitialization> Initialize(CancellationToken cancellationToken = default)
        {
            Logger.Debug().WriteLine("Initializing...");
            bool requiresRestart;
            try
            {
                requiresRestart = await StopService().ConfigureAwait(false);
            }
            catch (Exception)
            {
                requiresRestart = false;
            }

            if (requiresRestart)
            {
                Logger.Info().WriteLine("Service ShellHWDetection has been stopped and will be restarted once the operation finishes.");
            }

            return new InitializationResult
            {
                RestartHardwareService = requiresRestart
            };
        }

        public async Task Finish(IAppAttachVolumeCreationStrategyInitialization data, CancellationToken cancellationToken = default)
        {
            Logger.Debug().WriteLine("Finishing...");
            if (data is InitializationResult result && result.RestartHardwareService)
            {
                try
                {
                    await StartService(cancellationToken).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Logger.Warn().WriteLine("Could not restart the service ShellHWDetection.");
                    Logger.Debug().WriteLine(e);
                }
            }
        }

        public async Task CreateVolume(
            string packagePath,
            string volumePath,
            uint? sizeInMegaBytes,
            CancellationToken cancellationToken = default, 
            IProgress<ProgressData> progressReporter = null)
        {
            Logger.Debug().WriteLine($"Creating a new volume '{volumePath}' from package '{packagePath}' with desired size = {sizeInMegaBytes} MB...");

            if (packagePath == null)
            {
                throw new ArgumentNullException(nameof(packagePath), Resources.Localization.Packages_Error_EmptyPath);
            }

            if (volumePath == null)
            {
                throw new ArgumentNullException(nameof(volumePath), Resources.Localization.Packages_Error_EmptyVolumePath);
            }

            var packageFileInfo = new FileInfo(packagePath);
            if (!packageFileInfo.Exists)
            {
                throw new FileNotFoundException(string.Format(Resources.Localization.Packages_Error_FileMissing_Format, packagePath), packagePath);
            }

            switch (Path.GetExtension(volumePath).ToLowerInvariant())
            {
                case FileConstants.AppAttachVhdExtension:
                case FileConstants.AppAttachVhdxExtension:
                    break;
                default:
                    throw new NotSupportedException(string.Format(Resources.Localization.Packages_Error_DiskFormatNotSupported, Path.GetExtension(volumePath)));
            }

            var volumeFileInfo = new FileInfo(volumePath);
            if (volumeFileInfo.Directory != null && !volumeFileInfo.Directory.Exists)
            {
                volumeFileInfo.Directory.Create();
            }

            var tmpPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + Path.GetExtension(volumePath));

            using var progress = new WrappedProgress(progressReporter);

            // ReSharper disable once UnusedVariable
            var progressSize = sizeInMegaBytes <= 0 ? progress.GetChildProgress(30) : null;
            var progressInitializeDisk = progress.GetChildProgress(100);
            var progressExpand = progress.GetChildProgress(120);

            try
            {
                long minimumSize;
                if (sizeInMegaBytes.HasValue && sizeInMegaBytes.Value > 0)
                {
                    minimumSize = sizeInMegaBytes.Value;
                }
                else
                {
                    ISizeCalculator sizeCalculator = new VhdSizeCalculator();
                    minimumSize = await sizeCalculator.GetRequiredSize(packagePath, cancellationToken: cancellationToken).ConfigureAwait(false);
                }
                
                var wrapper = new DiskPartWrapper();

                try
                {
                    Logger.Debug().WriteLine("Getting drives (NTFS, ready) and volumes…");
                    var allDrives = DriveInfo.GetDrives().Where(d => d.IsReady && d.DriveFormat == "NTFS").Select(d => d.Name.ToLowerInvariant()).ToArray();
                    foreach (var item in allDrives)
                    {
                        Logger.Debug().WriteLine("* Drive: " + item);
                    }

                    var allVolumes = await MountVolumeHelper.GetVolumeIdentifiers().ConfigureAwait(false);
                    foreach (var item in allDrives)
                    {
                        Logger.Debug().WriteLine("* Volume: " + item);
                    }

                    await wrapper.CreateVhdAndAssignDriveLetter(tmpPath, minimumSize, cancellationToken, progressInitializeDisk).ConfigureAwait(false);
                    var newVolumes = (await MountVolumeHelper.GetVolumeIdentifiers().ConfigureAwait(false)).Except(allVolumes).ToArray();
                    foreach (var item in newVolumes)
                    {
                        Logger.Debug().WriteLine("* New volume: " + item);
                    }

                    var newDrives = DriveInfo.GetDrives().Where(d => d.IsReady && d.DriveFormat == "NTFS").Select(d => d.Name.ToLowerInvariant()).Except(allDrives).ToArray();
                    foreach (var item in newDrives)
                    {
                        Logger.Debug().WriteLine("* New drive: " + item);
                    }

                    if (newDrives.Length != 1 || newVolumes.Length != 1)
                    {
                        throw new InvalidOperationException(Resources.Localization.Packages_Error_Mount);
                    }

                    cancellationToken.ThrowIfCancellationRequested();
                    await this.ExpandMsix(packagePath, newDrives.First() + Path.GetFileNameWithoutExtension(packagePath), cancellationToken, progressExpand).ConfigureAwait(false);
                }
                finally
                {
                    await ExceptionGuard.Guard(wrapper.DismountVhd(tmpPath, cancellationToken)).ConfigureAwait(false);
                }

                if (File.Exists(tmpPath))
                {
                    File.Move(tmpPath, volumeFileInfo.FullName, true);
                }
            }
            finally
            {
                if (File.Exists(tmpPath))
                {
                    File.Delete(tmpPath);
                }
            }
        }

        private static Task StartService(CancellationToken cancellationToken, IProgress<ProgressData> progressReporter = default)
        {
            progressReporter?.Report(new ProgressData(0, Resources.Localization.Packages_Error_AppAttachFinishing));
            var serviceController = new ServiceController("ShellHWDetection");
            if (serviceController.Status != ServiceControllerStatus.Running)
            {
                cancellationToken.ThrowIfCancellationRequested();
                Logger.Info().WriteLine("Starting service ShellHWDetection…");

                try
                {
                    serviceController.Start();
                }
                catch
                {
                    // Some machines have issues and throw Access to the path 'x:\System Volume Information' is denied.
                    // This seems to be also the case in msixmgr.exe implementation.
                    // Since the VHD has been created, the issue will be discarded here and the process should continue.
                    Logger.Warn().WriteLine("Could not start ShellHWDetection service.");
                }
            }
            else
            {
                Logger.Debug().WriteLine("Service ShellHWDetection is already started…");
            }

            return Task.CompletedTask;
        }

        private async Task ExpandMsix(string sourcePath, string targetPath, CancellationToken cancellationToken, IProgress<ProgressData> progressReporter)
        {
            if (sourcePath == null)
            {
                throw new ArgumentNullException(nameof(sourcePath), Resources.Localization.Packages_Error_EmptyPath);
            }

            Logger.Info().WriteLine("Expanding MSIX…");
            progressReporter?.Report(new ProgressData(0, Resources.Localization.Packages_AppAttach_Expanding));
            cancellationToken.ThrowIfCancellationRequested();

            var dir = new DirectoryInfo(targetPath);
            var existing = dir.Exists ? dir.EnumerateDirectories().Select(d => d.Name.ToLowerInvariant()).ToArray() : Array.Empty<string>();

            await this.MsixMgr.Unpack(sourcePath, targetPath, cancellationToken, progressReporter).ConfigureAwait(false);
            progressReporter?.Report(new ProgressData(70, Resources.Localization.Packages_AppAttach_Expanding));

            var result = dir.EnumerateDirectories().Single(d => !existing.Contains(d.Name.ToLowerInvariant()));

            Logger.Info().WriteLine("Applying ACLs…");
            progressReporter?.Report(new ProgressData(80, Resources.Localization.Packages_AppAttach_ApplyingACL));
            await this.MsixMgr.ApplyAcls(result.FullName, cancellationToken).ConfigureAwait(false);
            progressReporter?.Report(new ProgressData(100, Resources.Localization.Packages_AppAttach_ApplyingACL));
        }

        private static Task<bool> StopService(IProgress<ProgressData> progressReporter = default)
        {
            Logger.Debug().WriteLine("Trying to stop service ShellHWDetection…");
            progressReporter?.Report(new ProgressData(0, Resources.Localization.Packages_AppAttach_Initializing));
            var serviceController = new ServiceController("ShellHWDetection");
            if (serviceController.Status == ServiceControllerStatus.Running)
            {
                serviceController.Stop();
                Logger.Debug().WriteLine("Service ShellHWDetection has been stopped…");
                return Task.FromResult(true);
            }

            Logger.Debug().WriteLine("Service ShellHWDetection did not require stopping…");
            return Task.FromResult(false);
        }

        private class InitializationResult : IAppAttachVolumeCreationStrategyInitialization
        {
            public bool RestartHardwareService { get; set; }
        }
    }
}