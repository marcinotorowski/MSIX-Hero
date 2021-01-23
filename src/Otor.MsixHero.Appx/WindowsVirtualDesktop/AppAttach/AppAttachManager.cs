// MSIX Hero
// Copyright (C) 2021 Marcin Otorowski
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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Otor.MsixHero.Appx.Signing;
using Otor.MsixHero.Infrastructure.Logging;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation.Enums;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.ThirdParty.Sdk;

namespace Otor.MsixHero.Appx.WindowsVirtualDesktop.AppAttach
{
    public class AppAttachManager : IAppAttachManager
    {
        private readonly ISigningManager signingManager;
        protected readonly MsixSdkWrapper MsixSdk = new MsixSdkWrapper();
        protected readonly MsixMgrWrapper MsixMgr = new MsixMgrWrapper();

        private static readonly ILog Logger = LogManager.GetLogger(typeof(AppAttachManager));
        private readonly ISelfElevationProxyProvider<ISigningManager> managerFactory;

        public AppAttachManager(ISelfElevationProxyProvider<ISigningManager> managerFactory)
        {
            this.managerFactory = managerFactory;
        }

        public AppAttachManager(ISigningManager signingManager)
        {
            this.signingManager = signingManager;
        }

        public async Task CreateVolume(string packagePath, string volumePath, uint vhdSize, bool extractCertificate, bool generateScripts, CancellationToken cancellationToken = default, IProgress<ProgressData> progressReporter = null)
        {
            if (packagePath == null)
            {
                throw new ArgumentNullException(nameof(packagePath));
            }

            if (volumePath == null)
            {
                throw new ArgumentNullException(nameof(packagePath));
            }

            var packageFileInfo = new FileInfo(packagePath);
            if (!packageFileInfo.Exists)
            {
                throw new FileNotFoundException($"File {packagePath} does not exist.", packagePath);
            }

            var volumeFileInfo = new FileInfo(volumePath);
            if (volumeFileInfo.Directory != null && !volumeFileInfo.Directory.Exists)
            {
                volumeFileInfo.Directory.Create();
            }

            var tmpPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".vhd");

            using (var progress = new WrappedProgress(progressReporter))
            {
                // ReSharper disable once UnusedVariable
                var progressSize = vhdSize <= 0 ? progress.GetChildProgress(30) : null;
                var progressStopService = progress.GetChildProgress(10);
                var progressInitializeDisk = progress.GetChildProgress(100);
                // var progressNewPartition = progress.GetChildProgress(30);
                // var progressFormatVolume = progress.GetChildProgress(80);
                var progressStartService = progress.GetChildProgress(10);
                var progressExpand = progress.GetChildProgress(120);
                var progressScripts = generateScripts ? progress.GetChildProgress(10) : null;

                try
                {
                    var minimumSize = vhdSize > 0 ? 1024 * 1024 * vhdSize : await GetVhdSize(packagePath, cancellationToken: cancellationToken).ConfigureAwait(false);

                    bool requiresRestart;
                    try
                    {
                        requiresRestart = await StopService(cancellationToken, progressStopService).ConfigureAwait(false);
                    }
                    catch (Exception)
                    {
                        requiresRestart = false;
                    }
                    
                    Guid volumeGuid;
                    var wrapper = new DiskPartWrapper();
                    string pkgFullName;

                    try
                    {
                        var allDrives = DriveInfo.GetDrives().Where(d => d.IsReady && d.DriveFormat == "NTFS").Select(d => d.Name.ToLowerInvariant()).ToArray();

                        var existing = await this.GetVolumeIdentifiers().ConfigureAwait(false);
                        await wrapper.CreateVhdAndAssignDriveLetter(tmpPath, minimumSize, cancellationToken, progressInitializeDisk).ConfigureAwait(false);
                        var newVolumes = (await this.GetVolumeIdentifiers().ConfigureAwait(false)).Except(existing).ToArray();

                        var newDrives = DriveInfo.GetDrives().Where(d => d.IsReady && d.DriveFormat == "NTFS").Select(d => d.Name.ToLowerInvariant()).Except(allDrives).ToArray();

                        if (newDrives.Length != 1 || newVolumes.Length != 1)
                        {
                            throw new InvalidOperationException("Could not mount the drive.");
                        }

                        volumeGuid = newVolumes[0];
                        cancellationToken.ThrowIfCancellationRequested();
                        pkgFullName = await this.ExpandMsix(packagePath, newDrives.First() + Path.GetFileNameWithoutExtension(packagePath), cancellationToken, progressExpand).ConfigureAwait(false);
                    }
                    finally
                    {
                        await wrapper.DismountVhd(tmpPath, cancellationToken).ConfigureAwait(false);

                        if (requiresRestart)
                        {
                            try
                            {
                                await StartService(cancellationToken, progressStartService).ConfigureAwait(false);
                            }
                            catch (Exception e)
                            {
                                Logger.Warn(e, "Could not restart the service ShellHWDetection.");
                            }
                        }
                    }

                    if (File.Exists(tmpPath))
                    {
                        if (generateScripts)
                        {
                            await CreateScripts(volumeFileInfo.FullName, Path.GetFileNameWithoutExtension(packagePath), volumeGuid, pkgFullName, cancellationToken, progressScripts).ConfigureAwait(false);
                        }

                        await CreateJson(volumeFileInfo.FullName, Path.GetFileNameWithoutExtension(packagePath), volumeGuid, pkgFullName, cancellationToken, progressScripts).ConfigureAwait(false);

                        if (extractCertificate)
                        {
                            ISigningManager certMgr = this.signingManager;
                            if (certMgr == null)
                            {
                                certMgr = await this.managerFactory.GetProxyFor(SelfElevationLevel.AsInvoker, cancellationToken).ConfigureAwait(false);
                            }
                            
                            cancellationToken.ThrowIfCancellationRequested();

                            // ReSharper disable once AssignNullToNotNullAttribute
                            await certMgr.ExtractCertificateFromMsix(packagePath, Path.Combine(volumeFileInfo.DirectoryName, Path.GetFileNameWithoutExtension(volumeFileInfo.FullName)) + ".cer", cancellationToken).ConfigureAwait(false);
                        }

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
        }

        private async Task<IList<Guid>> GetVolumeIdentifiers()
        {
            var psi = new ProcessStartInfo("mountvol") { CreateNoWindow = true, RedirectStandardOutput = true };
            // ReSharper disable once PossibleNullReferenceException
            var output = Process.Start(psi).StandardOutput;
            var allVolumes = await output.ReadToEndAsync().ConfigureAwait(false);

            var list = new List<Guid>();
            foreach (var item in allVolumes.Split(Environment.NewLine))
            {
                var io = item.IndexOf(@"\\?\Volume{", StringComparison.OrdinalIgnoreCase);
                if (io == -1)
                {
                    continue;
                }

                io -= 1;

                var guidString = item.Substring(io + @"\\?\Volume{".Length);
                var closing = guidString.IndexOf('}');
                if (closing == -1)
                {
                    continue;
                }

                guidString = guidString.Substring(0, closing + 1);
                if (Guid.TryParse(guidString, out var parsed))
                {
                    list.Add(parsed);
                }
            }

            return list;
        }

        private async Task<string> ExpandMsix(string msixPath, string vhdPath, CancellationToken cancellationToken, IProgress<ProgressData> progressReporter)
        {
            progressReporter?.Report(new ProgressData(0, "Expanding MSIX..."));
            cancellationToken.ThrowIfCancellationRequested();

            var dir = new DirectoryInfo(vhdPath);
            var existing = dir.Exists ? dir.EnumerateDirectories().Select(d => d.Name.ToLowerInvariant()).ToArray() : new string[0];

            await this.MsixMgr.Unpack(msixPath, vhdPath, cancellationToken, progressReporter).ConfigureAwait(false);
            progressReporter?.Report(new ProgressData(70, "Expanding MSIX..."));

            var result = dir.EnumerateDirectories().Single(d => !existing.Contains(d.Name.ToLowerInvariant()));

            progressReporter?.Report(new ProgressData(80, "Applying ACLs..."));
            await this.MsixMgr.ApplyAcls(result.FullName, cancellationToken).ConfigureAwait(false);
            progressReporter?.Report(new ProgressData(100, "Applying ACLs..."));
            return result.Name;
        }

        private static async Task CreateJson(string targetVhdPath, string packageParentFolder, Guid volumeGuid, string pkgFullName, CancellationToken cancellationToken, IProgress<ProgressData> progressReporter)
        {
            var vhdDir = Path.GetDirectoryName(targetVhdPath);

            if (vhdDir == null)
            {
                throw new InvalidOperationException("The target path must contain a directory.");
            }

            var vhdFileName = Path.GetFileNameWithoutExtension(targetVhdPath);

            var jsonObject = new JObject
            {
                ["vhdFileName"] = Path.GetFileName(targetVhdPath),
                ["parentFolder"] = packageParentFolder,
                ["packageName"] = pkgFullName,
                ["volumeGuid"] = volumeGuid.ToString("D"),
                ["msixJunction"] = @"C:\temp\AppAttach"
            };

            var jsonArray = new JArray { jsonObject };

            var contentJson = jsonArray.ToString(Formatting.Indented);
            await File.WriteAllTextAsync(Path.Combine(vhdDir, "app-attach.json"), contentJson, Encoding.UTF8, cancellationToken).ConfigureAwait(false);
        }

        private static async Task CreateScripts(string targetVhdPath, string packageParentFolder, Guid volumeGuid, string pkgFullName, CancellationToken cancellationToken, IProgress<ProgressData> progressReporter)
        {
            progressReporter?.Report(new ProgressData(0, "Creating PowerShell scripts..."));

            var templateStage = Path.Combine(BundleHelper.TemplatesPath, "AppAttachStage.ps1");

            if (!File.Exists(templateStage))
            {
                throw new FileNotFoundException($"Required template {templateStage} was not found.", templateStage);
            }

            var templateRegister = Path.Combine(BundleHelper.TemplatesPath, "AppAttachRegister.ps1");

            if (!File.Exists(templateRegister))
            {
                throw new FileNotFoundException($"Required template {templateRegister} was not found.", templateRegister);
            }

            var templateDeregister = Path.Combine(BundleHelper.TemplatesPath, "AppAttachDeregister.ps1");

            if (!File.Exists(templateDeregister))
            {
                throw new FileNotFoundException($"Required template {templateDeregister} was not found.", templateDeregister);
            }

            var templateDestage = Path.Combine(BundleHelper.TemplatesPath, "AppAttachDestage.ps1");

            if (!File.Exists(templateDestage))
            {
                throw new FileNotFoundException($"Required template {templateDestage} was not found.", templateDestage);
            }

            
            var vhdDir = Path.GetDirectoryName(targetVhdPath);

            if (vhdDir == null)
            {
                throw new InvalidOperationException("The target path must contain a directory.");
            }

            var vhdFileName = Path.GetFileNameWithoutExtension(targetVhdPath);

            var files = new List<Tuple<string, string>>()
            {
                new Tuple<string, string>(templateStage, Path.Combine(vhdDir, "stage.ps1")),
                new Tuple<string, string>(templateDestage, Path.Combine(vhdDir, "destage.ps1")),
                new Tuple<string, string>(templateDeregister, Path.Combine(vhdDir, "deregister.ps1")),
                new Tuple<string, string>(templateRegister, Path.Combine(vhdDir, "register.ps1")),
            };

            foreach (var file in files)
            {
                using (var source = File.OpenRead(file.Item1))
                {
                    using (var target = File.OpenWrite(file.Item2))
                    {
                        await source.CopyToAsync(target, cancellationToken).ConfigureAwait(false);
                        await source.FlushAsync(cancellationToken).ConfigureAwait(false);
                    }
                }
            }
        }

        private static Task StartService(CancellationToken cancellationToken, IProgress<ProgressData> progressReporter)
        {
            return Task.Run(() =>
                {
                    progressReporter?.Report(new ProgressData(0, "Finishing..."));
                    var serviceController = new ServiceController("ShellHWDetection");
                    if (serviceController.Status != ServiceControllerStatus.Running)
                    {
                        Logger.Debug("Trying to start service ShellHWDetection...");
                        serviceController.Start();
                    }
                    else
                    {
                        Logger.Debug("Service ShellHWDetection is already started...");
                    }
                },
                cancellationToken);
        }

        private static Task<bool> StopService(CancellationToken cancellationToken, IProgress<ProgressData> progressReporter)
        {
            return Task.Run(() =>
                {
                    Logger.Debug("Trying to stop service ShellHWDetection...");
                    progressReporter?.Report(new ProgressData(0, "Initializing..."));
                    var serviceController = new ServiceController("ShellHWDetection");
                    if (serviceController.Status == ServiceControllerStatus.Running)
                    {
                        serviceController.Stop();
                        Logger.Debug("Service ShellHWDetection has been stopped...");
                        return true;
                    }

                    Logger.Debug("Service ShellHWDetection did not require stopping...");
                    return false;
                },
                cancellationToken);
        }


        private static Task<long> GetVhdSize(string packagePath, double extraMargin = 0.2, CancellationToken cancellationToken = default)
        {
            const long reserved = 16 * 1024 * 1024;
            const long minSize = 32 * 1024 * 1024;

            var total = 0L;
            using (var archive = ZipFile.OpenRead(packagePath))
            {
                foreach (var item in archive.Entries)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    total += item.Length;

                    if (item.Length <= 512)
                    {
                        //Small files are contained in MFT
                        continue;
                    }

                    var surplus = item.Length % 4096;
                    if (surplus != 0)
                    {
                        total += 4096 - surplus;
                    }

                }
            }

            return Task.FromResult(Math.Max(minSize, (long)(total * (1 + extraMargin)) + reserved));
        }
    }
}
