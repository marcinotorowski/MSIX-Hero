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
using otor.msixhero.lib.Domain.Appx.Manifest.Full;
using otor.msixhero.lib.Domain.Appx.Volume;
using otor.msixhero.lib.Infrastructure.Interop;
using otor.msixhero.lib.Infrastructure.Logging;
using otor.msixhero.lib.Infrastructure.Progress;

namespace otor.msixhero.lib.BusinessLayer.Managers.Volumes
{
    public class AppxVolumeManager : IAppxVolumeManager
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(AppxVolumeManager));

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
            
            if (letter != null)
            {
                var drive = DriveInfo.GetDrives().First(d => d.RootDirectory.FullName.StartsWith(letter, StringComparison.OrdinalIgnoreCase));
                if (drive != null)
                {
                    appxVolume.IsDriveReady = drive.IsReady;
                    appxVolume.DiskLabel = drive.VolumeLabel;
                    appxVolume.Capacity = drive.TotalSize;
                    appxVolume.AvailableFreeSpace = drive.AvailableFreeSpace;
                }
            }

            return appxVolume;
        }

        private const int FILE_SHARE_READ = 1;
        private const int FILE_SHARE_WRITE = 2;

        private const int CREATION_DISPOSITION_OPEN_EXISTING = 3;
        private const int FILE_FLAG_BACKUP_SEMANTICS = 0x02000000;

        [DllImport("kernel32.dll", EntryPoint = "GetFinalPathNameByHandleW", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int GetFinalPathNameByHandle(IntPtr handle, [In, Out] StringBuilder path, int bufLen, int flags);

        [DllImport("kernel32.dll", EntryPoint = "CreateFileW", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern SafeFileHandle CreateFile(string lpFileName, int dwDesiredAccess, int dwShareMode, IntPtr SecurityAttributes, int dwCreationDisposition, int dwFlagsAndAttributes, IntPtr hTemplateFile);

        private static string GetSymbolicLinkTarget(System.IO.DirectoryInfo symlink)
        {
            var directoryHandle = CreateFile(symlink.FullName, 0, 2, System.IntPtr.Zero, CREATION_DISPOSITION_OPEN_EXISTING, FILE_FLAG_BACKUP_SEMANTICS, System.IntPtr.Zero);
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
                throw new DirectoryNotFoundException("Directory was not found.");
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

            var baseType = item.BaseObject.GetType();
            var name = (string)baseType.GetProperty("Name")?.GetValue(item.BaseObject);
            var packageStorePath = (string)baseType.GetProperty("PackageStorePath")?.GetValue(item.BaseObject);

            



            var letter = packageStorePath != null && packageStorePath.Length > 2 && packageStorePath[1] == ':' ? packageStorePath.Substring(0, 1) + ":\\" : null;

            var appxVolume = new AppxVolume { Name = name, PackageStorePath = packageStorePath };

            if (letter != null)
            {
                var drive = DriveInfo.GetDrives().First(d => d.RootDirectory.FullName.StartsWith(letter, StringComparison.OrdinalIgnoreCase));
                if (drive != null)
                {
                    appxVolume.IsDriveReady = drive.IsReady;
                    appxVolume.DiskLabel = drive.VolumeLabel;
                    appxVolume.Capacity = drive.TotalSize;
                    appxVolume.AvailableFreeSpace = drive.AvailableFreeSpace;
                }
            }

            return appxVolume;
        }

        public async Task<List<AppxVolume>> GetAll(CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            using var session = await PowerShellSession.CreateForAppxModule().ConfigureAwait(false);
            using var command = session.AddCommand("Get-AppxVolume");

            var results = await session.InvokeAsync().ConfigureAwait(false);
            if (!results.Any())
            {
                return new List<AppxVolume>();
            }

            var list = new List<AppxVolume>();

            foreach (var item in results)
            {
                var baseType = item.BaseObject.GetType();
                var name = (string)baseType.GetProperty("Name")?.GetValue(item.BaseObject);
                var packageStorePath = (string)baseType.GetProperty("PackageStorePath")?.GetValue(item.BaseObject);
                var isOffline = (bool?)baseType.GetProperty("IsOffline")?.GetValue(item.BaseObject) == true;
                var isSystemVolume = (bool?)baseType.GetProperty("IsSystemVolume")?.GetValue(item.BaseObject) == true;

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

            var results = await session.InvokeAsync().ConfigureAwait(false);
            var obj = results.FirstOrDefault();
            if (obj == null)
            {
                throw new InvalidOperationException($"Volume {drivePath} could not be created.");
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
            await session.InvokeAsync().ConfigureAwait(false);
        }

        public async Task Mount(AppxVolume volume, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            using var session = await PowerShellSession.CreateForAppxModule().ConfigureAwait(false);
            using var command = session.AddCommand("Mount-AppxVolume");
            command.AddParameter("Volume", volume.Name);
            await session.InvokeAsync().ConfigureAwait(false);
        }

        public async Task Dismount(AppxVolume volume, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            using var session = await PowerShellSession.CreateForAppxModule().ConfigureAwait(false);
            using var command = session.AddCommand("Dismount-AppxVolume");
            command.AddParameter("Volume", volume.Name);
            await session.InvokeAsync().ConfigureAwait(false);
        }

        public async Task MovePackageToVolume(string volumePackagePath, string packageFullName, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            using var session = await PowerShellSession.CreateForAppxModule().ConfigureAwait(false);
            using var command = session.AddCommand("Move-AppxPackage");
            command.AddParameter("Package", packageFullName);
            command.AddParameter("Volume", volumePackagePath);

            Logger.Debug($"Executing Move-AppxPackage -Package \"{packageFullName}\" -Volume \"{volumePackagePath}\"...");
            await session.InvokeAsync().ConfigureAwait(false);
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
            var p1 = wrappedProgress.GetChildProgress(50);
            var p2 = wrappedProgress.GetChildProgress(50);
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
            var p1 = wrappedProgress.GetChildProgress(50);
            var p2 = wrappedProgress.GetChildProgress(50);
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
            var p1 = wrappedProgress.GetChildProgress(50);
            var p2 = wrappedProgress.GetChildProgress(50);
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
            Logger.Info("Setting volume {0} as default...", volume.Name);
            await session.InvokeAsync(progress).ConfigureAwait(false);
        }

        public async Task SetDefault(string drivePath, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            using var wrappedProgress = new WrappedProgress(progress);
            var p1 = wrappedProgress.GetChildProgress(50);
            var p2 = wrappedProgress.GetChildProgress(50);
            var allVolumes = await this.GetAll(cancellationToken, p1).ConfigureAwait(false);

            drivePath = GetDriveLetterFromPath(drivePath);
            Logger.Info("Looking for volume {0}...", drivePath);
            foreach (var item in allVolumes)
            {
                Logger.Debug(" * Found volume '{0}'", item.PackageStorePath);
            }

            var volume = allVolumes.FirstOrDefault(v => GetDriveLetterFromPath(v.PackageStorePath) == drivePath);
            if (volume == null)
            {
                throw new DriveNotFoundException($"Could not find volume '{drivePath}'");
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
