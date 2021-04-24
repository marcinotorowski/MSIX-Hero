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
        protected readonly MsixMgrWrapper MsixMgr = new MsixMgrWrapper();
        
        private static readonly ILog Logger = LogManager.GetLogger(typeof(AppAttachManager));
        
        private readonly ISigningManager signingManager;
        private readonly ISelfElevationProxyProvider<ISigningManager> managerFactory;

        public AppAttachManager(ISelfElevationProxyProvider<ISigningManager> managerFactory)
        {
            this.managerFactory = managerFactory;
        }

        public AppAttachManager(ISigningManager signingManager)
        {
            this.signingManager = signingManager;
        }

        public Task CreateVolume(string packagePath, string volumePath, uint vhdSize, bool extractCertificate, bool generateScripts, CancellationToken cancellationToken = default, IProgress<ProgressData> progressReporter = null)
        {
            return this.CreateVolume(packagePath, volumePath, vhdSize, AppAttachVolumeType.Vhd, extractCertificate, generateScripts, cancellationToken, progressReporter);
        }

        public async Task CreateVolumes(
            IReadOnlyCollection<string> packagePaths, 
            string volumeDirectory, 
            AppAttachVolumeType type = AppAttachVolumeType.Vhd,
            bool extractCertificate = false, 
            bool generateScripts = true, 
            CancellationToken cancellationToken = default,
            IProgress<ProgressData> progressReporter = null)
        {
            if (volumeDirectory == null)
            {
                throw new ArgumentNullException(nameof(volumeDirectory));
            }

            var diskExtension = type.ToString("G").ToLowerInvariant();

            MsixMgrWrapper.FileType? msixMgrVolumeType;
            switch (type)
            {
                case AppAttachVolumeType.Vhd:
                    msixMgrVolumeType = MsixMgrWrapper.FileType.Vhd;
                    break;
                case AppAttachVolumeType.Cim:
                    msixMgrVolumeType = MsixMgrWrapper.FileType.Cim;
                    break;
                case AppAttachVolumeType.Vhdx:
                    msixMgrVolumeType = MsixMgrWrapper.FileType.Vhdx;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            if (!Directory.Exists(volumeDirectory))
            {
                Logger.Info("Creating directory {0}...", volumeDirectory);
                Directory.CreateDirectory(volumeDirectory);
            }

            var allPackagesCount = packagePaths.Count;
            var currentPackage = 0;
            
            foreach (var packagePath in packagePaths)
            {
                var currentVolumeDirectory = type == AppAttachVolumeType.Cim ? Path.Combine(volumeDirectory, Path.GetFileNameWithoutExtension(packagePath)) : volumeDirectory;
                
                var packageProgressReporter = new RangeProgress(progressReporter, (int)(currentPackage * 100.0 / allPackagesCount), (int)((currentPackage + 1) * 100.0 / allPackagesCount));
                currentPackage++;
                
                var packageFileInfo = new FileInfo(packagePath);
                if (!packageFileInfo.Exists)
                {
                    throw new FileNotFoundException($"File {packagePath} does not exist.", packagePath);
                }

                var volumeFileInfo = new FileInfo(Path.Combine(currentVolumeDirectory, Path.GetFileNameWithoutExtension(packagePath) + "." + diskExtension));
                if (volumeFileInfo.Exists)
                {
                    volumeFileInfo.Delete();
                }
                else if (volumeFileInfo.Directory?.Exists == false)
                {
                    volumeFileInfo.Directory.Create();
                }

                packageProgressReporter.Report(new ProgressData(0, $"Analyzing {Path.GetFileName(packagePath)}..."));

                uint size;
                if (type == AppAttachVolumeType.Cim)
                {
                    size = (uint)(await GetRequiredSize(packagePath, cancellationToken:cancellationToken).ConfigureAwait(false) / 1024 / 1024);
                }
                else
                {
                    size = (uint)(await GetVhdSize(packagePath, cancellationToken: cancellationToken).ConfigureAwait(false) / 1024 / 1024);
                }
                
                var rootDirectory = Path.GetFileNameWithoutExtension(packagePath);
                
                Logger.Debug("Unpacking {0} with MSIXMGR...", packagePath);
                packageProgressReporter.Report(new ProgressData(20, $"Unpacking {Path.GetFileName(packagePath)}..."));
                await this.MsixMgr.Unpack(
                    packagePath,
                    volumeFileInfo.FullName,
                    msixMgrVolumeType,
                    size,
                    true,
                    true,
                    rootDirectory,
                    cancellationToken).ConfigureAwait(false);

                if (extractCertificate)
                {
                    progressReporter?.Report(new ProgressData(80, $"Extracting certificate from {Path.GetFileName(packagePath)}..."));
                    var actualSigningManager = this.signingManager ?? await this.managerFactory.GetProxyFor(SelfElevationLevel.AsInvoker, cancellationToken).ConfigureAwait(false);

                    cancellationToken.ThrowIfCancellationRequested();

                    // ReSharper disable once AssignNullToNotNullAttribute
                    await actualSigningManager.ExtractCertificateFromMsix(packagePath, Path.Combine(volumeFileInfo.DirectoryName, Path.GetFileNameWithoutExtension(volumeFileInfo.FullName)) + ".cer", cancellationToken).ConfigureAwait(false);
                }
            }

            if (type == AppAttachVolumeType.Cim)
            {
                // Currently JSON and PS1 are only supported for VHD(X)...
            }
            else
            {
                var jsonData = new List<JsonData>();
                
                if (generateScripts)
                {
                    progressReporter?.Report(new ProgressData(90, "Creating scripts..."));
                    await CreateScripts(volumeDirectory, cancellationToken).ConfigureAwait(false);
                }

                progressReporter?.Report(new ProgressData(95, "Creating JSON file..."));
                foreach (var volumePath in packagePaths.Select(p => Path.Combine(volumeDirectory, Path.GetFileNameWithoutExtension(p) + "." + diskExtension)))
                {
                    var volumeData = await this.GetExpandedPackageData(volumePath, cancellationToken).ConfigureAwait(false);
                    var volumeGuid = volumeData.Item1;
                    var volumeMsixFolderName = volumeData.Item2;
                    
                    jsonData.Add(new JsonData(Path.GetFileName(volumePath), Path.GetFileNameWithoutExtension(volumePath), volumeGuid, volumeMsixFolderName));
                }

                var jsonPath = Path.Combine(volumeDirectory, "app-attach.json");
                
                await CreateJson(jsonPath, jsonData, cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task CreateVolume(
            string packagePath,
            string volumePath,
            uint size,
            AppAttachVolumeType type = AppAttachVolumeType.Vhd,
            bool extractCertificate = false,
            bool generateScripts = true,
            CancellationToken cancellationToken = default,
            IProgress<ProgressData> progressReporter = null)
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
                Logger.Info("Creating directory " + volumeFileInfo.Directory.FullName);
                volumeFileInfo.Directory.Create();
            }

            MsixMgrWrapper.FileType? msixMgrVolumeType;
            switch (type)
            {
                case AppAttachVolumeType.Vhd:
                    msixMgrVolumeType = MsixMgrWrapper.FileType.Vhd;
                    break;
                case AppAttachVolumeType.Cim:
                    msixMgrVolumeType = MsixMgrWrapper.FileType.Cim;
                    break;
                case AppAttachVolumeType.Vhdx:
                    msixMgrVolumeType = MsixMgrWrapper.FileType.Vhdx;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            progressReporter?.Report(new ProgressData(0, "Getting package size..."));
            if (size <= 0)
            {
                if (type == AppAttachVolumeType.Cim)
                {
                    size = (uint)(await GetRequiredSize(packagePath, cancellationToken: cancellationToken).ConfigureAwait(false) / 1024 / 1024);
                }
                else
                {
                    size = (uint)(await GetVhdSize(packagePath, cancellationToken: cancellationToken).ConfigureAwait(false) / 1024 / 1024);
                }
            }
            
            var rootDirectory = Path.GetFileNameWithoutExtension(packagePath);

            if (File.Exists(volumePath))
            {
                File.Delete(volumePath);
            }

            Logger.Debug("Unpacking with MSIXMGR...");
            progressReporter?.Report(new ProgressData(20, "Unpacking MSIX..."));
            await this.MsixMgr.Unpack(
                packagePath,
                volumePath,
                msixMgrVolumeType,
                size,
                true,
                true,
                rootDirectory,
                cancellationToken).ConfigureAwait(false);

            if (extractCertificate)
            {
                progressReporter?.Report(new ProgressData(80, "Extracting certificate..."));
                var actualSigningManager = this.signingManager ?? await this.managerFactory.GetProxyFor(SelfElevationLevel.AsInvoker, cancellationToken).ConfigureAwait(false);

                cancellationToken.ThrowIfCancellationRequested();

                // ReSharper disable once AssignNullToNotNullAttribute
                await actualSigningManager.ExtractCertificateFromMsix(packagePath, Path.Combine(volumeFileInfo.DirectoryName, Path.GetFileNameWithoutExtension(volumeFileInfo.FullName)) + ".cer", cancellationToken).ConfigureAwait(false);
            }
            
            if (type == AppAttachVolumeType.Cim)
            {
                // Currently JSON and PS1 are only supported for VHD(X)...
            }
            else if (File.Exists(volumePath))
            {
                var volumeData = await this.GetExpandedPackageData(volumePath, cancellationToken).ConfigureAwait(false);
                var volumeGuid = volumeData.Item1;
                var volumeMsixFolderName = volumeData.Item2;

                if (generateScripts)
                {
                    await CreateScripts(volumeFileInfo.Directory?.FullName, cancellationToken).ConfigureAwait(false);
                }
                
                await CreateJson(
                    // ReSharper disable once AssignNullToNotNullAttribute
                    Path.Combine(Path.GetDirectoryName(volumePath), "app-attach.json"),
                    new []
                    {
                        new JsonData(volumeFileInfo.Name, rootDirectory, volumeGuid, volumeMsixFolderName)
                    },
                    cancellationToken).ConfigureAwait(false);
            }
        }
        
        private async Task<Tuple<Guid, string>> GetExpandedPackageData(string volumePath, CancellationToken cancellationToken)
        {
            Logger.Debug("Getting GUID and extracted path from volume {0}...", volumePath);
            var wrapper = new DiskPartWrapper();

            var mountedVhd = false;
            try
            {
                var allDrives = DriveInfo.GetDrives().Where(d => d.IsReady && d.DriveFormat == "NTFS").Select(d => d.Name.ToLowerInvariant()).ToArray();

                var existing = await this.GetVolumeIdentifiers().ConfigureAwait(false);

                Logger.Debug("Mounting {0}...", volumePath);
                await wrapper.MountVhd(volumePath, cancellationToken).ConfigureAwait(false);
                mountedVhd = true;
                
                var newVolumes = (await this.GetVolumeIdentifiers().ConfigureAwait(false)).Except(existing).ToArray();

                var newDrives = DriveInfo.GetDrives().Where(d => d.IsReady && d.DriveFormat == "NTFS").Select(d => d.Name.ToLowerInvariant()).Except(allDrives).ToArray();

                if (newDrives.Length != 1 || newVolumes.Length != 1)
                {
                    throw new InvalidOperationException("Could not mount the drive.");
                }

                var dir = new DirectoryInfo(newDrives[0]);
                var msixFolderName = dir.EnumerateDirectories().FirstOrDefault()?.EnumerateDirectories().FirstOrDefault()?.Name;
                if (msixFolderName == null)
                {
                    throw new InvalidOperationException("Could not read the content of the mounted file.");
                }

                return new Tuple<Guid, string>(newVolumes[0], msixFolderName);
            }
            finally
            {
                if (mountedVhd)
                {
                    Logger.Debug("Dismounting {0}...", volumePath);
                    await wrapper.DismountVhd(volumePath, cancellationToken).ConfigureAwait(false);
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

        private static async Task CreateJson(string jsonPath, IEnumerable<JsonData> jsonData, CancellationToken cancellationToken)
        {
            var jsonArray = new JArray();

            foreach (var item in jsonData)
            {
                Logger.Debug("Creating JSON file {0}...", jsonPath);

                var jsonObject = new JObject
                {
                    ["vhdFileName"] = item.VhdFileName,
                    ["parentFolder"] = item.RootName,
                    ["packageName"] = item.PackageFolderName,
                    ["volumeGuid"] = item.VolumeGuid.ToString("D"),
                    ["msixJunction"] = @"C:\temp\AppAttach"
                };

                jsonArray.Add(jsonObject);
            }

            var contentJson = jsonArray.ToString(Formatting.Indented);
            await File.WriteAllTextAsync(jsonPath, contentJson, Encoding.UTF8, cancellationToken).ConfigureAwait(false);
        }

        private static async Task CreateScripts(string directory, CancellationToken cancellationToken)
        {
            if (directory == null)
            {
                throw new InvalidOperationException("The target path must be a directory.");
            }

            Logger.Debug("Creating scripts in {0}...", directory);
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
            
            var files = new List<Tuple<string, string>>
            {
                new (templateStage, Path.Combine(directory, "stage.ps1")),
                new(templateDestage, Path.Combine(directory, "destage.ps1")),
                new (templateDeregister, Path.Combine(directory, "deregister.ps1")),
                new (templateRegister, Path.Combine(directory, "register.ps1")),
            };

            foreach (var file in files)
            {
                Logger.Debug("Copying {0} to {1}...", file.Item1, file.Item2);
                await using var source = File.OpenRead(file.Item1);
                await using var target = File.OpenWrite(file.Item2);
                await source.CopyToAsync(target, cancellationToken).ConfigureAwait(false);
                await source.FlushAsync(cancellationToken).ConfigureAwait(false);
            }
        }
        
        private static Task<long> GetVhdSize(string packagePath, double extraMargin = 0.2, CancellationToken cancellationToken = default)
        {
            const long reserved = 16 * 1024 * 1024;
            
            var total = 0L;
            Logger.Debug("Determining required VHD size for package {0}...", packagePath);
            
            using (var archive = ZipFile.OpenRead(packagePath))
            {
                foreach (var item in archive.Entries)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    total += item.Length;

                    if (item.Length <= 512)
                    {
                        // Small files are contained in MFT
                        continue;
                    }

                    var surplus = item.Length % 4096;
                    if (surplus != 0)
                    {
                        total += 4096 - surplus;
                    }

                }
            }

            return Task.FromResult((long)(total * (1 + extraMargin)) + reserved);
        }

        private static Task<long> GetRequiredSize(string packagePath, double extraMargin = 0.2, CancellationToken cancellationToken = default)
        {
            var total = 0L;
            Logger.Debug("Determining required VHD size for package {0}...", packagePath);

            using (var archive = ZipFile.OpenRead(packagePath))
            {
                foreach (var item in archive.Entries)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    total += item.Length;
                }
            }

            return Task.FromResult((long)(total * (1 + extraMargin)));
        }
        
        private struct JsonData
        {
            public JsonData(string vhdFileName, string rootName, Guid volumeGuid, string packageFolderName)
            {
                this.VhdFileName = vhdFileName;
                this.RootName = rootName;
                this.VolumeGuid = volumeGuid;
                this.PackageFolderName = packageFolderName;
            }

            public string VhdFileName;
            public string RootName;
            public Guid VolumeGuid;
            public string PackageFolderName;
        }

    }
}
