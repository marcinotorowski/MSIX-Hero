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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Management.Deployment;
using Microsoft.Win32.SafeHandles;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;
using Otor.MsixHero.Appx.Volumes.Entities;
using Dapplo.Log;
using Otor.MsixHero.Appx.Exceptions;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.ThirdParty.PowerShell;
using Otor.MsixHero.Appx.Packaging.Installation;
using Otor.MsixHero.Appx.Packaging.Interop;
using Windows.Media.Ocr;

namespace Otor.MsixHero.Appx.Volumes
{
    public class AppxVolumeService : IAppxVolumeService
    {
        private static readonly LogSource Logger = new();

        public async Task<AppxVolume> GetDefault(CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var pkgManager = PackageManagerSingleton.Instance;
            var defaultVolume = pkgManager.GetDefaultPackageVolume();
            if (defaultVolume == null)
            {
                return null;
            }

            var vol = await this.GetVolume(defaultVolume, false, cancellationToken).ConfigureAwait(false);
            vol.IsDefault = true;
            return vol;
        }
        
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

            var pkgManager = PackageManagerSingleton.Instance;
            var allVolumes = await AsyncOperationHelper.ConvertToTask(pkgManager.GetPackageVolumesAsync(), cancellationToken).ConfigureAwait(false);
            var item = allVolumes.FirstOrDefault(v => path.StartsWith(v.PackageStorePath?.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar));
            return await this.GetVolume(item, true, cancellationToken).ConfigureAwait(false);
        }

        public async Task<List<AppxVolume>> GetAll(CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var pkgManager = PackageManagerSingleton.Instance;
            var allVolumes = await AsyncOperationHelper.ConvertToTask(pkgManager.GetPackageVolumesAsync(), cancellationToken).ConfigureAwait(false);
            return await this.GetVolumes(allVolumes, true, cancellationToken).ToListAsync(cancellationToken).ConfigureAwait(false);
        }
        
        public async Task<AppxVolume> Add(string drivePath, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var pkgVolume = await AsyncOperationHelper.ConvertToTask(PackageManagerSingleton.Instance.AddPackageVolumeAsync(drivePath), cancellationToken).ConfigureAwait(false);
            if (pkgVolume == null)
            {
                throw new InvalidOperationException(string.Format(Resources.Localization.Packages_Error_VolumeCreation_Format, drivePath));
            }

            return await this.GetVolume(pkgVolume, true, cancellationToken).ConfigureAwait(false);
        }

        private async Task<AppxVolume> GetVolume(PackageVolume volume, bool resolveDefaultVolume, CancellationToken cancellationToken)
        {
            if (volume == null)
            {
                return null;
            }

            return await this.GetVolumes(new[] { volume }, resolveDefaultVolume, cancellationToken).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task Delete(AppxVolume volume, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var findVolume = PackageManagerSingleton.Instance.FindPackageVolume(volume.Name);
            if (findVolume == null)
            {
                return;
            }

            try
            {
                await AsyncOperationHelper.ConvertToTask(PackageManagerSingleton.Instance.RemovePackageVolumeAsync(findVolume), string.Format(Resources.Localization.Volumes_Removing_Format, volume.DiskLabel), cancellationToken, progress).ConfigureAwait(false);
            }
            catch (COMException e)
            {
                switch ((uint)e.HResult)
                {
                    case 0x80073D0C: // ERROR_INSTALL_VOLUME_NOT_EMPTY
                        throw new MsixHeroException(Resources.Localization.Volumes_RemovingError_NotEmpty, e);
                }
            }
        }

        public async Task Mount(AppxVolume volume, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var findVolume = PackageManagerSingleton.Instance.FindPackageVolume(volume.Name);
            if (findVolume == null)
            {
                throw new InvalidOperationException();
            }

            await AsyncOperationHelper.ConvertToTask(PackageManagerSingleton.Instance.SetPackageVolumeOnlineAsync(findVolume), string.Format(Resources.Localization.Volumes_Mounting_Format, volume.DiskLabel), cancellationToken, progress).ConfigureAwait(false);
        }

        public async Task Dismount(AppxVolume volume, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var findVolume = PackageManagerSingleton.Instance.FindPackageVolume(volume.Name);
            if (findVolume == null)
            {
                throw new InvalidOperationException();
            }

            await AsyncOperationHelper.ConvertToTask(PackageManagerSingleton.Instance.SetPackageVolumeOfflineAsync(findVolume), string.Format(Resources.Localization.Volumes_Mounting_Format, volume.DiskLabel), cancellationToken, progress).ConfigureAwait(false);
        }

        public async Task MovePackageToVolume(AppxVolume volume, string packageFullName, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            if (volume == null)
            {
                throw new ArgumentNullException(nameof(volume));
            }

            if (packageFullName == null)
            {
                throw new ArgumentNullException(nameof(packageFullName));
            }

            var targetVolumes = await AsyncOperationHelper.ConvertToTask(PackageManagerSingleton.Instance.GetPackageVolumesAsync(), cancellationToken).ConfigureAwait(false);
            var targetVolume = targetVolumes.First(v => string.Equals(v.PackageStorePath, volume.PackageStorePath, StringComparison.OrdinalIgnoreCase));

            var r = await AsyncOperationHelper.ConvertToTask(
                PackageManagerSingleton.Instance.MovePackageToVolumeAsync(
                    packageFullName,
                    DeploymentOptions.None,
                    targetVolume),
                Resources.Localization.Volumes_Moving,
                cancellationToken, 
                progress).ConfigureAwait(false);

            if (r.IsRegistered)
            {
                return;
            }

            if (r.ExtendedErrorCode != null)
            {
                throw r.ExtendedErrorCode;
            }
        }

        public Task MovePackageToVolume(AppxVolume volume, AppxPackage package, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            return this.MovePackageToVolume(volume, package.FullName, cancellationToken, progress);
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

        public Task SetDefault(AppxVolume volume, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var findVolume = PackageManagerSingleton.Instance.FindPackageVolume(volume.Name);
            if (findVolume == null)
            {
                return Task.FromException(new InvalidOperationException());
            }

            try
            {
                PackageManagerSingleton.Instance.SetDefaultPackageVolume(findVolume);
            }
            catch (Exception e)
            {
                return Task.FromException(e);
            }

            return Task.CompletedTask;
        }

        public async Task SetDefault(string drivePath, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            using var wrappedProgress = new WrappedProgress(progress);
            var p1 = wrappedProgress.GetChildProgress();
            var p2 = wrappedProgress.GetChildProgress();
            var allVolumes = await this.GetAll(cancellationToken, p1).ConfigureAwait(false);

            drivePath = GetDriveLetterFromPath(drivePath);
            Logger.Info().WriteLine("Looking for volume {0}…", drivePath);
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
                matching.Capacity = (ulong)drive.TotalSize;
                matching.AvailableFreeSpace = (ulong)drive.AvailableFreeSpace;
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

        private async IAsyncEnumerable<AppxVolume> GetVolumes(IEnumerable<PackageVolume> allVolumes, bool resolveDefaultVolume, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var list = new List<AppxVolume>();

            var defaultVol = resolveDefaultVolume ? await this.GetDefault(cancellationToken).ConfigureAwait(false) : null;
            foreach (var item in allVolumes)
            {
                if (!item.IsAppxInstallSupported)
                {
                    continue;
                }

                if (!item.IsFullTrustPackageSupported)
                {
                    continue;
                }

                if (!item.SupportsHardLinks)
                {
                    continue;
                }

                if (item.MountPoint == item.PackageStorePath)
                {
                    // workaround?
                    Logger.Warn().WriteLine($"Ignoring drive {item.PackageStorePath}...");
                    continue;
                }

                cancellationToken.ThrowIfCancellationRequested();

                var packageStorePath = item.PackageStorePath;
                if (string.IsNullOrEmpty(packageStorePath))
                {
                    Logger.Warn().WriteLine("Empty path for " + item);
                    continue;
                }

                var isOffline = item.IsOffline;
                var name = item.Name;
                var isSystemVolume = item.IsSystemVolume;

                list.Add(new AppxVolume
                {
                    Name = name,
                    PackageStorePath = packageStorePath,
                    IsDefault = string.Equals(item.Name, defaultVol?.Name, StringComparison.OrdinalIgnoreCase),
                    IsOffline = isOffline,
                    IsSystem = isSystemVolume,
                    AvailableFreeSpace = await AsyncOperationHelper.ConvertToTask(item.GetAvailableSpaceAsync(), cancellationToken).ConfigureAwait(false)
                });
            }

            var drives = DriveInfo.GetDrives();
            foreach (var drive in list.Where(c => c.PackageStorePath.IndexOf(":\\", StringComparison.Ordinal) == 1))
            {
                cancellationToken.ThrowIfCancellationRequested();

                var letter = drive.PackageStorePath.Substring(0, 3);
                var matchingDrive = drives.FirstOrDefault(d => string.Equals(d.RootDirectory.FullName, letter, StringComparison.OrdinalIgnoreCase));
                if (matchingDrive?.IsReady != true)
                {
                    continue;
                }

                drive.Capacity = (ulong)matchingDrive.TotalSize;
                drive.DiskLabel = matchingDrive.VolumeLabel;
                drive.IsDriveReady = true;
            }

            foreach (var item in list.OrderBy(l => l.PackageStorePath))
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return item;
            }
        }
    }
}
