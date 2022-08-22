// MSIX Hero
// Copyright (C) 2022 Marcin Otorowski
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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Otor.MsixHero.Appx.Signing;
using Otor.MsixHero.Appx.WindowsVirtualDesktop.AppAttach.MountVol;
using Otor.MsixHero.Appx.WindowsVirtualDesktop.AppAttach.Strategy;
using Dapplo.Log;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Infrastructure.ThirdParty.Sdk;

namespace Otor.MsixHero.Appx.WindowsVirtualDesktop.AppAttach
{
    public class AppAttachManager : IAppAttachManager
    {
        protected readonly MsixMgrWrapper MsixMgr = new MsixMgrWrapper();
        
        private static readonly LogSource Logger = new();        
        private readonly ISigningManager signingManager;
        private readonly IConfigurationService configurationService;
        
        public AppAttachManager(ISigningManager signingManager, IConfigurationService configurationService) : this(configurationService)
        {
            this.signingManager = signingManager;
        }

        public AppAttachManager(IConfigurationService configurationService)
        {
            this.configurationService = configurationService;
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
            
            if (!Directory.Exists(volumeDirectory))
            {
                Logger.Info().WriteLine("Creating directory {0}…", volumeDirectory);
                Directory.CreateDirectory(volumeDirectory);
            }

            var allPackagesCount = packagePaths.Count;
            var currentPackage = 0;

            var volumeCreationStrategy = await this.GetVolumeCreationStrategy(type).ConfigureAwait(false);

            IAppAttachVolumeCreationStrategyInitialization initialization = null;
            try
            {
                initialization = await volumeCreationStrategy.Initialize(cancellationToken).ConfigureAwait(false);
                
                foreach (var packagePath in packagePaths)
                {
                    var currentVolumeDirectory = type == AppAttachVolumeType.Cim ? Path.Combine(volumeDirectory, Path.GetFileNameWithoutExtension(packagePath)) : volumeDirectory;

                    var packageProgressReporter = new RangeProgress(progressReporter, (int)(currentPackage * 80.0 / allPackagesCount), (int)((currentPackage + 1) * 80.0 / allPackagesCount));
                    currentPackage++;

                    var packageFileInfo = new FileInfo(packagePath);
                    if (!packageFileInfo.Exists)
                    {
                        throw new FileNotFoundException(string.Format(Resources.Localization.Packages_Error_FileMissing_Format, packagePath), packagePath);
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

                    packageProgressReporter.Report(new ProgressData(0, string.Format(Resources.Localization.Packages_Analyzing, Path.GetFileName(packagePath))));

                    await volumeCreationStrategy.CreateVolume(packagePath, volumeFileInfo.FullName, null, cancellationToken, packageProgressReporter).ConfigureAwait(false);

                    if (extractCertificate)
                    {
                        packageProgressReporter.Report(new ProgressData(100, string.Format(Resources.Localization.Packages_ExtractingCertFrom_Format, Path.GetFileName(packagePath))));
                        cancellationToken.ThrowIfCancellationRequested();

                        // ReSharper disable once AssignNullToNotNullAttribute
                        await this.signingManager.ExtractCertificateFromMsix(packagePath, Path.Combine(volumeFileInfo.DirectoryName, Path.GetFileNameWithoutExtension(volumeFileInfo.FullName)) + ".cer", cancellationToken).ConfigureAwait(false);
                    }
                }

                if (type == AppAttachVolumeType.Cim)
                {
                    // Currently JSON and PS1 are only supported for VHD(X)…
                }
                else
                {
                    var jsonData = new List<JsonData>();

                    if (generateScripts)
                    {
                        progressReporter?.Report(new ProgressData(90, Resources.Localization.Packages_AppAttach_CreatingScripts));
                        await CreateScripts(volumeDirectory, cancellationToken).ConfigureAwait(false);
                    }

                    progressReporter?.Report(new ProgressData(95, Resources.Localization.Packages_AppAttach_CreatingJson));
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
            finally
            {
                await volumeCreationStrategy.Finish(initialization, cancellationToken).ConfigureAwait(false);
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
                throw new ArgumentNullException(nameof(volumePath));
            }

            var packageFileInfo = new FileInfo(packagePath);
            if (!packageFileInfo.Exists)
            {
                Logger.Error().WriteLine($"File {packagePath} does not exist.");
                throw new FileNotFoundException(string.Format(Resources.Localization.Packages_Error_FileMissing_Format, packagePath), packagePath);
            }

            var volumeFileInfo = new FileInfo(volumePath);
            if (volumeFileInfo.Directory != null && !volumeFileInfo.Directory.Exists)
            {
                Logger.Info().WriteLine($"Creating directory {volumeFileInfo.Directory.FullName}…");
                volumeFileInfo.Directory.Create();
            }

            var volumeCreationStrategy = await this.GetVolumeCreationStrategy(type);
            
            if (File.Exists(volumePath))
            {
                Logger.Info().WriteLine($"Deleting existing file {volumePath}…");
                File.Delete(volumePath);
            }

            progressReporter?.Report(new ProgressData(20, Resources.Localization.Packages_AppAttach_UnpackingMsix));

            IAppAttachVolumeCreationStrategyInitialization initialization = null;
            try
            {
                initialization = await volumeCreationStrategy.Initialize(cancellationToken).ConfigureAwait(false);

                await volumeCreationStrategy.CreateVolume(packagePath, volumePath, size, cancellationToken).ConfigureAwait(false);

                if (extractCertificate)
                {
                    progressReporter?.Report(new ProgressData(80, Resources.Localization.Packages_ExtractingCert));
                    
                    cancellationToken.ThrowIfCancellationRequested();

                    // ReSharper disable once AssignNullToNotNullAttribute
                    await this.signingManager.ExtractCertificateFromMsix(packagePath, Path.Combine(volumeFileInfo.DirectoryName, Path.GetFileNameWithoutExtension(volumeFileInfo.FullName)) + ".cer", cancellationToken).ConfigureAwait(false);
                }

                if (type == AppAttachVolumeType.Cim)
                {
                    // Currently JSON and PS1 are only supported for VHD(X)…
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
                        new[]
                        {
                            new JsonData(volumeFileInfo.Name, Path.GetFileNameWithoutExtension(packagePath), volumeGuid, volumeMsixFolderName)
                        },
                        cancellationToken).ConfigureAwait(false);
                }
            }
            finally
            {
                await volumeCreationStrategy.Finish(initialization, cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task<IAppAttachVolumeCreationStrategy> GetVolumeCreationStrategy(AppAttachVolumeType type)
        {
            IAppAttachVolumeCreationStrategy volumeCreationStrategy;
            if (type == AppAttachVolumeType.Cim)
            {
                volumeCreationStrategy = new AppAttachVolumeCreationMsixMgrStrategy();
            }
            else
            {
                var config = await this.configurationService.GetCurrentConfigurationAsync().ConfigureAwait(false);
                if (config.AppAttach?.UseMsixMgrForVhdCreation == true)
                {
                    // Forced MSIXMGR by the user configuration (non-default)
                    volumeCreationStrategy = new AppAttachVolumeCreationMsixMgrStrategy();
                }
                else
                {
                    volumeCreationStrategy = new AppAttachVolumeCreationMsixHeroStrategy();
                }
            }

            return volumeCreationStrategy;
        }

        private async Task<Tuple<Guid, string>> GetExpandedPackageData(string volumePath, CancellationToken cancellationToken)
        {
            Logger.Debug().WriteLine("Getting GUID and extracted path from volume {0}…", volumePath);
            var wrapper = new DiskPartWrapper();

            var mountedVhd = false;
            try
            {
                var allDrives = DriveInfo.GetDrives().Where(d => d.IsReady && d.DriveFormat == "NTFS").Select(d => d.Name.ToLowerInvariant()).ToArray();

                var existing = await MountVolumeHelper.GetVolumeIdentifiers().ConfigureAwait(false);

                Logger.Debug().WriteLine("Mounting {0}…", volumePath);
                await wrapper.MountVhd(volumePath, cancellationToken).ConfigureAwait(false);
                mountedVhd = true;

                var newVolumes = (await MountVolumeHelper.GetVolumeIdentifiers().ConfigureAwait(false)).Except(existing).ToArray();

                var newDrives = DriveInfo.GetDrives().Where(d => d.IsReady && d.DriveFormat == "NTFS").Select(d => d.Name.ToLowerInvariant()).Except(allDrives).ToArray();

                if (newDrives.Length != 1 || newVolumes.Length != 1)
                {
                    throw new InvalidOperationException(Resources.Localization.Packages_Error_Mount);
                }

                var dir = new DirectoryInfo(newDrives[0]);
                var msixFolderName = dir.EnumerateDirectories().FirstOrDefault()?.EnumerateDirectories().FirstOrDefault()?.Name;
                if (msixFolderName == null)
                {
                    throw new InvalidOperationException(Resources.Localization.Packages_Error_MountContent);
                }

                return new Tuple<Guid, string>(newVolumes[0], msixFolderName);
            }
            finally
            {
                if (mountedVhd)
                {
                    Logger.Debug().WriteLine("Dismounting {0}…", volumePath);
                    await wrapper.DismountVhd(volumePath, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        private static async Task CreateJson(string jsonPath, IEnumerable<JsonData> jsonData, CancellationToken cancellationToken)
        {
            var jsonArray = new JArray();

            foreach (var item in jsonData)
            {
                Logger.Debug().WriteLine("Creating JSON file {0}…", jsonPath);

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
                throw new InvalidOperationException(Resources.Localization.Packages_Error_TargetNotDir);
            }

            Logger.Debug().WriteLine("Creating scripts in {0}…", directory);
            var templateStage = Path.Combine(BundleHelper.TemplatesPath, "AppAttachStage.ps1");

            if (!File.Exists(templateStage))
            {
                throw new FileNotFoundException(string.Format(Resources.Localization.Packages_Error_TemplateNotFound_Format, templateStage), templateStage);
            }

            var templateRegister = Path.Combine(BundleHelper.TemplatesPath, "AppAttachRegister.ps1");

            if (!File.Exists(templateRegister))
            {
                throw new FileNotFoundException(string.Format(Resources.Localization.Packages_Error_TemplateNotFound_Format, templateRegister), templateRegister);
            }

            var templateDeregister = Path.Combine(BundleHelper.TemplatesPath, "AppAttachDeregister.ps1");

            if (!File.Exists(templateDeregister))
            {
                throw new FileNotFoundException(string.Format(Resources.Localization.Packages_Error_TemplateNotFound_Format, templateDeregister), templateDeregister);
            }

            var templateDestage = Path.Combine(BundleHelper.TemplatesPath, "AppAttachDestage.ps1");

            if (!File.Exists(templateDestage))
            {
                throw new FileNotFoundException(string.Format(Resources.Localization.Packages_Error_TemplateNotFound_Format, templateDestage), templateDestage);
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
                Logger.Debug().WriteLine("Copying {0} to {1}…", file.Item1, file.Item2);
                await using var source = File.OpenRead(file.Item1);
                await using var target = File.OpenWrite(file.Item2);
                await source.CopyToAsync(target, cancellationToken).ConfigureAwait(false);
                await source.FlushAsync(cancellationToken).ConfigureAwait(false);
            }
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
