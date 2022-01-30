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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;
using Otor.MsixHero.Appx.Volumes.Entities;
using Dapplo.Log;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.ThirdParty.PowerShell;

namespace Otor.MsixHero.Appx.Volumes
{
    public class AppxVolumeManager : IAppxVolumeManager
    {
        private static readonly LogSource Logger = new();
        public async Task<AppxVolume> GetDefault(CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            using var session = await PowerShellSession.CreateForAppxModule().ConfigureAwait(false);
            using var command = session.AddCommand("Get-AppxDefaultVolume");

            var result = await session.InvokeAsync(progress).ConfigureAwait(false);

            var item = result.FirstOrDefault();
            if (item == null)
            {
                return null;
            }

            var baseType = item.BaseObject.GetType();
            var name = (string) baseType.GetProperty("Name")?.GetValue(item.BaseObject);
            var packageStorePath = (string) baseType.GetProperty("PackageStorePath")?.GetValue(item.BaseObject);

            var letter = packageStorePath != null && packageStorePath.Length > 2 && packageStorePath[1] == ':' ? packageStorePath.Substring(0, 1) + ":\\" : null;

            var appxVolume = new AppxVolume { Name = name, PackageStorePath = packageStorePath };

            if (letter == null) return appxVolume;
            var drive = DriveInfo.GetDrives().First(d => d.RootDirectory.FullName.StartsWith(letter, StringComparison.OrdinalIgnoreCase));
            appxVolume.IsDriveReady = drive.IsReady;
            appxVolume.DiskLabel = drive.VolumeLabel;
            appxVolume.Capacity = drive.TotalSize;
            appxVolume.AvailableFreeSpace = drive.AvailableFreeSpace;

            return appxVolume;
        }

        // ReSharper disable once UnusedMember.Local
        // ReSharper disable once InconsistentNaming
        private const int FILE_SHARE_READ = 1;
        // ReSharper disable once UnusedMember.Local
        // ReSharper disable once InconsistentNaming
        private const int FILE_SHARE_WRITE = 2;

        // ReSharper disable once InconsistentNaming
        private const int CREATION_DISPOSITION_OPEN_EXISTING = 3;
        // ReSharper disable once InconsistentNaming
        private const int FILE_FLAG_BACKUP_SEMANTICS = 0x02000000;

        [DllImport("kernel32.dll", EntryPoint = "GetFinalPathNameByHandleW", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int GetFinalPathNameByHandle(IntPtr handle, [In, Out] StringBuilder path, int bufLen, int flags);

        [DllImport("kernel32.dll", EntryPoint = "CreateFileW", CharSet = CharSet.Unicode, SetLastError = true)]
        // ReSharper disable once InconsistentNaming
        private static extern SafeFileHandle CreateFile(string lpFileName, int dwDesiredAccess, int dwShareMode, IntPtr SecurityAttributes, int dwCreationDisposition, int dwFlagsAndAttributes, IntPtr hTemplateFile);

        private static string GetSymbolicLinkTarget(FileSystemInfo symlink)
        {
            var directoryHandle = CreateFile(symlink.FullName, 0, 2, IntPtr.Zero, CREATION_DISPOSITION_OPEN_EXISTING, FILE_FLAG_BACKUP_SEMANTICS, IntPtr.Zero);
            if (directoryHandle.IsInvalid)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            var path = new StringBuilder(512);
            var size = GetFinalPathNameByHandle(directoryHandle.DangerousGetHandle(), path, path.Capacity, 0);
            if (size < 0)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            // The remarks section of GetFinalPathNameByHandle mentions the return being prefixed with "\\?\"
            // More information about "\\?\" here -> http://msdn.microsoft.com/en-us/library/aa365247(v=VS.85).aspx
            if (path[0] == '\\' && path[1] == '\\' && path[2] == '?' && path[3] == '\\')
            {
                return path.ToString().Substring(4);
            }

            return path.ToString();
        }

        public async Task<AppxVolume> GetVolumeForPath(string path, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var di = new DirectoryInfo(path);
            if (!di.Exists)
            {
                throw new DirectoryNotFoundException(string.Format(Resources.Localization.Packages_Error_DirectoryMissing_Format, di.FullName));
            }

            if (di.Attributes.HasFlag(FileAttributes.ReparsePoint))
            {
                var target = GetSymbolicLinkTarget(di);
                path = target;
            }

            using var session = await PowerShellSession.CreateForAppxModule().ConfigureAwait(false);
            using var command = session.AddCommand("Get-AppPackageVolume");
            command.AddParameter("Path", path);

            var result = await session.InvokeAsync(progress).ConfigureAwait(false);

            var item = result.FirstOrDefault();
            if (item == null)
            {
                return null;
            }
            
            var name = (string)item.Properties.FirstOrDefault(p => p.Name == "Name")?.Value;
            var packageStorePath = (string)item.Properties.FirstOrDefault(p => p.Name == "PackageStorePath")?.Value;
            
            var letter = packageStorePath != null && packageStorePath.Length > 2 && packageStorePath[1] == ':' ? packageStorePath.Substring(0, 1) + ":\\" : null;

            var appxVolume = new AppxVolume { Name = name, PackageStorePath = packageStorePath };

            if (letter == null)
            {
                return appxVolume;
            }
            
            var drive = DriveInfo.GetDrives().FirstOrDefault(d => d.RootDirectory.FullName.StartsWith(letter, StringComparison.OrdinalIgnoreCase));
            if (drive == null)
            {
                return appxVolume;
            }
                
            appxVolume.IsDriveReady = drive.IsReady;
            appxVolume.DiskLabel = drive.VolumeLabel;
            appxVolume.Capacity = drive.TotalSize;
            appxVolume.AvailableFreeSpace = drive.AvailableFreeSpace;

            return appxVolume;
        }

        public async Task<List<AppxVolume>> GetAll(CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            using var session = await PowerShellSession.CreateForAppxModule().ConfigureAwait(false);
            using var command = session.AddCommand("Get-AppxVolume");

            var results = await session.InvokeAsync(progress).ConfigureAwait(false);
            if (!results.Any())
            {
                return new List<AppxVolume>();
            }

            var list = new List<AppxVolume>();

            foreach (var item in results)
            {
                var packageStorePath = (string)item.Properties.FirstOrDefault(p => p.Name == "PackageStorePath")?.Value;
                if (string.IsNullOrEmpty(packageStorePath))
                {
                    Logger.Warn().WriteLine("Empty path for " + item);
                    continue;
                }
                
                var isOffline = true == (bool?)item.Properties.FirstOrDefault(p => p.Name == "IsOffline")?.Value;
                var name = (string)item.Properties.FirstOrDefault(p => p.Name == "Name")?.Value;
                var isSystemVolume = true == (bool?)item.Properties.FirstOrDefault(p => p.Name == "IsSystemVolume")?.Value;
                
                list.Add(new AppxVolume { Name = name, PackageStorePath = packageStorePath, IsOffline = isOffline, IsSystem = isSystemVolume });
            }

            var drives = DriveInfo.GetDrives();
            foreach (var drive in list.Where(c => c.PackageStorePath.IndexOf(":\\", StringComparison.Ordinal) == 1))
            {
                var letter = drive.PackageStorePath.Substring(0, 3);
                var matchingDrive = drives.FirstOrDefault(d => string.Equals(d.RootDirectory.FullName, letter, StringComparison.OrdinalIgnoreCase));
                if (matchingDrive?.IsReady == true)
                {
                    drive.Capacity = matchingDrive.TotalSize;
                    drive.AvailableFreeSpace = matchingDrive.AvailableFreeSpace;
                    drive.DiskLabel = matchingDrive.VolumeLabel;
                    drive.IsDriveReady = true;
                }
            }

            return list.OrderBy(l => l.PackageStorePath).ToList();
        }
        
        public async Task<AppxVolume> Add(string drivePath, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            using var session = await PowerShellSession.CreateForAppxModule().ConfigureAwait(false);
            using var command = session.AddCommand("Add-AppxVolume");
            command.AddParameter("Path", drivePath);

            var results = await session.InvokeAsync(progress).ConfigureAwait(false);
            var obj = results.FirstOrDefault();
            if (obj == null)
            {
                throw new InvalidOperationException(string.Format(Resources.Localization.Packages_Error_VolumeCreation_Format, drivePath));
            }

            var baseType = obj.BaseObject.GetType();
            var name = (string)baseType.GetProperty("Name")?.GetValue(obj.BaseObject);
            var packageStorePath = (string)baseType.GetProperty("PackageStorePath")?.GetValue(obj.BaseObject);

            return new AppxVolume { Name = name, PackageStorePath = packageStorePath };
        }

        public async Task Delete(AppxVolume volume, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            using var session = await PowerShellSession.CreateForAppxModule().ConfigureAwait(false);
            using var command = session.AddCommand("Remove-AppxVolume");
            command.AddParameter("Volume", volume.Name);
            await session.InvokeAsync(progress).ConfigureAwait(false);
        }

        public async Task Mount(AppxVolume volume, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            using var session = await PowerShellSession.CreateForAppxModule().ConfigureAwait(false);
            using var command = session.AddCommand("Mount-AppxVolume");
            command.AddParameter("Volume", volume.Name);
            await session.InvokeAsync(progress).ConfigureAwait(false);
        }

        public async Task Dismount(AppxVolume volume, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            using var session = await PowerShellSession.CreateForAppxModule().ConfigureAwait(false);
            using var command = session.AddCommand("Dismount-AppxVolume");
            command.AddParameter("Volume", volume.Name);
            await session.InvokeAsync(progress).ConfigureAwait(false);
        }

        public async Task MovePackageToVolume(string volumePackagePath, string packageFullName, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            using var session = await PowerShellSession.CreateForAppxModule().ConfigureAwait(false);
            using var command = session.AddCommand("Move-AppxPackage");
            command.AddParameter("Package", packageFullName);
            command.AddParameter("Volume", volumePackagePath);

            Logger.Debug().WriteLine($"Executing Move-AppxPackage -Package \"{packageFullName}\" -Volume \"{volumePackagePath}\"...");
            await session.InvokeAsync(progress).ConfigureAwait(false);
        }

        public Task MovePackageToVolume(AppxVolume volume, AppxPackage package, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            if (volume == null)
            {
                throw new ArgumentNullException(nameof(volume));
            }

            if (package == null)
            {
                throw new ArgumentNullException(nameof(package));
            }

            return this.MovePackageToVolume(volume.Name, package.FullName, cancellationToken, progress);
        }

        public async Task Delete(string name, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            using var wrappedProgress = new WrappedProgress(progress);
            var p1 = wrappedProgress.GetChildProgress();
            var p2 = wrappedProgress.GetChildProgress();
            var allVolumes = await this.GetAll(cancellationToken, p1).ConfigureAwait(false);
            
            var volume = allVolumes.FirstOrDefault(v => string.Equals(name, v.Name, StringComparison.OrdinalIgnoreCase));
            if (volume == null)
            {
                return;
            }

            await this.Delete(volume, cancellationToken, p2).ConfigureAwait(false);
        }

        public async Task Mount(string name, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            using var wrappedProgress = new WrappedProgress(progress);
            var p1 = wrappedProgress.GetChildProgress();
            var p2 = wrappedProgress.GetChildProgress();
            var allVolumes = await this.GetAll(cancellationToken, p1).ConfigureAwait(false);
            
            var volume = allVolumes.FirstOrDefault(v => string.Equals(name, v.Name, StringComparison.OrdinalIgnoreCase));
            if (volume == null)
            {
                return;
            }

            await this.Mount(volume, cancellationToken, p2).ConfigureAwait(false);
        }

        public async Task Dismount(string name, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            using var wrappedProgress = new WrappedProgress(progress);
            var p1 = wrappedProgress.GetChildProgress();
            var p2 = wrappedProgress.GetChildProgress();
            var allVolumes = await this.GetAll(cancellationToken, p1).ConfigureAwait(false);
            
            var volume = allVolumes.FirstOrDefault(v => string.Equals(name, v.Name, StringComparison.OrdinalIgnoreCase));
            if (volume == null)
            {
                return;
            }

            await this.Dismount(volume, cancellationToken, p2).ConfigureAwait(false);
        }

        public async Task SetDefault(AppxVolume volume, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            using var session = await PowerShellSession.CreateForAppxModule().ConfigureAwait(false);
            using var command = session.AddCommand("Set-AppxDefaultVolume");
            command.AddParameter("Volume", volume.Name);
            Logger.Info().WriteLine("Setting volume {0} as default...", volume.Name);
            await session.InvokeAsync(progress).ConfigureAwait(false);
        }

        public async Task SetDefault(string drivePath, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            using var wrappedProgress = new WrappedProgress(progress);
            var p1 = wrappedProgress.GetChildProgress();
            var p2 = wrappedProgress.GetChildProgress();
            var allVolumes = await this.GetAll(cancellationToken, p1).ConfigureAwait(false);

            drivePath = GetDriveLetterFromPath(drivePath);
            Logger.Info().WriteLine("Looking for volume {0}...", drivePath);
            foreach (var item in allVolumes)
            {
                Logger.Debug().WriteLine(" * Found volume '{0}'", item.PackageStorePath);
            }

            var volume = allVolumes.FirstOrDefault(v => GetDriveLetterFromPath(v.PackageStorePath) == drivePath);
            if (volume == null)
            {
                throw new DriveNotFoundException(string.Format(Resources.Localization.Packages_Error_VolumeNotFound_Format, drivePath));
            }

            await this.SetDefault(volume, cancellationToken, p2).ConfigureAwait(false);
        }

        public async Task<List<AppxVolume>> GetAvailableDrivesForAppxVolume(bool onlyUnused, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var drives = await Task.Run(DriveInfo.GetDrives, cancellationToken).ConfigureAwait(false);

            var result = new List<AppxVolume>();
            foreach (var drive in drives.Where(d => d.IsReady))
            {
                cancellationToken.ThrowIfCancellationRequested();

                var fn = drive.RootDirectory.FullName;
                if (fn.Length < 2)
                {
                    continue;
                }

                if (fn[1] == ':')
                {
                    result.Add(new AppxVolume { PackageStorePath = fn.Substring(0, 1) + ":\\" });
                }
            }

            cancellationToken.ThrowIfCancellationRequested();
            if (onlyUnused)
            {
                var allVolumes = await this.GetAll(cancellationToken, progress).ConfigureAwait(false);
                var letters = new HashSet<string>(allVolumes.Select(v => GetDriveLetterFromPath(v.PackageStorePath)), StringComparer.OrdinalIgnoreCase);

                for (var i = result.Count - 1; i >= 0; i--)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (letters.Contains(result[i].PackageStorePath))
                    {
                        result.RemoveAt(i);
                    }
                }
            }

            if (!result.Any())
            {
                return result;
            }

            foreach (var drive in DriveInfo.GetDrives().Where(d => d.IsReady))
            {
                var driveLetter = GetDriveLetterFromPath(drive.RootDirectory.FullName);
                if (driveLetter == null)
                {
                    continue;
                }

                var matching = result.FirstOrDefault(d => string.Equals(d.PackageStorePath, driveLetter, StringComparison.OrdinalIgnoreCase));
                if (matching == null)
                {
                    continue;
                }

                matching.IsDriveReady = drive.IsReady;
                matching.DiskLabel = drive.VolumeLabel;
                matching.Capacity = drive.TotalSize;
                matching.AvailableFreeSpace = drive.AvailableFreeSpace;
            }

            return result;
        }

        private static string GetDriveLetterFromPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            if (path.Length == 1)
            {
                if (!char.IsLetter(path[0]))
                {
                    return null;
                }

                return path.ToUpper() + ":\\";
            }

            if (path[1] == ':')
            {
                return char.ToUpperInvariant(path[0]) + ":\\";
            }

            return null;
        }
    }
}
