using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Interop;
using otor.msixhero.lib.Infrastructure.Logging;
using otor.msixhero.lib.Infrastructure.Progress;
using otor.msixhero.lib.Infrastructure.Wrappers;

namespace otor.msixhero.lib.BusinessLayer.Appx.AppAttach
{
    public class AppAttach : IAppAttach
    {
        protected readonly MsixSdkWrapper MsixSdk = new MsixSdkWrapper();
        protected readonly MsixMgrWrapper MsixMgr = new MsixMgrWrapper();
        private static readonly ILog Logger = LogManager.GetLogger(typeof(AppAttach));
        
        public async Task CreateVolume(string packagePath, string volumePath, uint vhdSize, bool generateScripts, CancellationToken cancellationToken = default, IProgress<ProgressData> progressReporter = null)
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

            try
            {
                /*
                 *Stop-Service -Name ShellHWDetection
        New-VHD -SizeBytes 20MB -Path c:\temp\msix-fuk.vhd -Dynamic -Confirm:$false
        $vhdObject = Mount-VHD c:\temp\msix-fuk.vhd -Passthru
        $disk = Initialize-Disk -Passthru -Number $vhdObject.Number -PartitionStyle GPT
        $partition = New-Partition -AssignDriveLetter -UseMaximumSize -DiskNumber $disk.Number
        Format-Volume -FileSystem NTFS -Confirm:$false -DriveLetter $partition.DriveLetter -Force
        Start-Service -Name ShellHWDetection
        Dismount-VHD -DiskNumber $disk.Number -Passthru
        .\msixmgr.exe -Unpack -packagePath E:\temp\MSIX.Commander_1.0.6.0-x64.msix -destination "f:\MSIX" -applyacls
                 *
                 */

                using (var progress = new WrappedProgress(progressReporter))
                {
                    var progressSize = vhdSize <= 0 ? progress.GetChildProgress(50) : null;
                    var progressStopService = progress.GetChildProgress(10);
                    var progressNewVhd = progress.GetChildProgress(50);
                    var progressMountVhd = progress.GetChildProgress(30);
                    var progressInitializeDisk = progress.GetChildProgress(20);
                    var progressNewPartition = progress.GetChildProgress(30);
                    var progressFormatVolume = progress.GetChildProgress(80);
                    var progressStartService = progress.GetChildProgress(10);
                    var progressExpand = progress.GetChildProgress(80);
                    var progressDismount = progress.GetChildProgress(10);
                    var progressScripts = generateScripts ? progress.GetChildProgress(10) : null;

                    long minimumSize;
                    if (vhdSize <= 0)
                    {
                        minimumSize = await this.GetVhdSize(packagePath, 1.0, cancellationToken, progressSize).ConfigureAwait(false);
                    }
                    else
                    {
                        minimumSize = 1024 * 1024 * vhdSize;
                    }

                    var requiresRestart = await this.StopService(cancellationToken, progressStopService).ConfigureAwait(false);
                    MountVhdData? moundVhdData = null;

                    try
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        await this.NewVhd((int)(10 * Math.Ceiling(0.1 * minimumSize / 1024 / 1024)), tmpPath, cancellationToken, progressNewVhd).ConfigureAwait(false);

                        cancellationToken.ThrowIfCancellationRequested();
                        moundVhdData = await this.MountVhd(tmpPath, cancellationToken, progressMountVhd).ConfigureAwait(false);

                        cancellationToken.ThrowIfCancellationRequested();
                        var initializedDiskNumber = await this.InitializeDisk(moundVhdData.Value.DiskNumber, cancellationToken, progressInitializeDisk).ConfigureAwait(false);

                        cancellationToken.ThrowIfCancellationRequested();
                        var driveLetter = await this.NewPartition(initializedDiskNumber, cancellationToken, progressNewPartition).ConfigureAwait(false);

                        cancellationToken.ThrowIfCancellationRequested();
                        await this.FormatVolume(driveLetter, cancellationToken, progressFormatVolume).ConfigureAwait(false);

                        cancellationToken.ThrowIfCancellationRequested();
                        var packageFullName = await this.ExpandMsix(packagePath, driveLetter + ":\\" + Path.GetFileNameWithoutExtension(packagePath), cancellationToken, progressExpand).ConfigureAwait(false);

                        cancellationToken.ThrowIfCancellationRequested();

                        if (generateScripts)
                        {
                            await this.CreateScripts(volumeFileInfo.FullName, packageFullName, Path.GetFileNameWithoutExtension(packagePath), moundVhdData.Value.Guid, cancellationToken, progressScripts).ConfigureAwait(false);
                        }
                    }
                    finally
                    {
                        if (moundVhdData.HasValue)
                        {
                            try
                            {
                                await this.DismountVhd(moundVhdData.Value.DiskNumber, cancellationToken, progressDismount);
                            }
                            catch (Exception e)
                            {
                                Logger.Warn(e, "Could not dismount drive number {0}.", moundVhdData.Value.DiskNumber);
                            }
                        }

                        if (requiresRestart)
                        {
                            try
                            {
                                await this.StartService(cancellationToken, progressStartService).ConfigureAwait(false);
                            }
                            catch (Exception e)
                            {
                                Logger.Warn(e, "Could not restart the service ShellHWDetection.");
                            }
                        }
                    }
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

        private async Task FormatVolume(string partitionLetter, CancellationToken cancellationToken, IProgress<ProgressData> progressReporter)
        {
            progressReporter?.Report(new ProgressData(0, "Formatting partition..."));

            using (var powerShellSession = await PowerShellSession.CreateForModule("Storage", true).ConfigureAwait(false))
            {
                cancellationToken.ThrowIfCancellationRequested();
                // New-VHD -SizeBytes <size>MB -Path c:\temp\<name>.vhd -Dynamic -Confirm:$false
                using (var cmd = powerShellSession.AddCommand("Format-Volume"))
                {
                    cmd.AddParameter("DriveLetter", partitionLetter);
                    cmd.AddParameter("FileSystem", "NTFS");
                    cmd.AddParameter("Confirm", false);
                    cmd.AddParameter("Force", false);
                    await powerShellSession.InvokeAsync(new ProgressNumberOnly(progressReporter, "Formatting partition...")).ConfigureAwait(false);
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }

            progressReporter?.Report(new ProgressData(100, "Formatting partition..."));
        }

        private async Task<string> NewPartition(uint diskNumber, CancellationToken cancellationToken, IProgress<ProgressData> progressReporter)
        {
            string partitionDriveLetter;
            progressReporter?.Report(new ProgressData(0, "Creating partition..."));

            using (var powerShellSession = await PowerShellSession.CreateForModule("Storage", true).ConfigureAwait(false))
            {
                cancellationToken.ThrowIfCancellationRequested();
                // New-VHD -SizeBytes <size>MB -Path c:\temp\<name>.vhd -Dynamic -Confirm:$false
                using (var cmd = powerShellSession.AddCommand("New-Partition"))
                {
                    cmd.AddParameter("DiskNumber", diskNumber);
                    cmd.AddParameter("UseMaximumSize");
                    cmd.AddParameter("AssignDriveLetter");
                    var result = await powerShellSession.InvokeAsync(new ProgressNumberOnly(progressReporter, "Creating partition...")).ConfigureAwait(false);

                    cancellationToken.ThrowIfCancellationRequested();

                    if (!result.Any())
                    {
                        throw new InvalidOperationException();
                    }

                    partitionDriveLetter = result[0].Members["DriveLetter"].Value.ToString();
                }
            }

            progressReporter?.Report(new ProgressData(100, "Creating partition..."));
            return partitionDriveLetter;
        }

        private async Task<uint> InitializeDisk(uint diskNumber, CancellationToken cancellationToken, IProgress<ProgressData> progressReporter)
        {
            uint initializedDiskNumber;
            progressReporter?.Report(new ProgressData(0, "Initializing virtual hard disk..."));

            using (var powerShellSession = await PowerShellSession.CreateForModule("Storage", true).ConfigureAwait(false))
            {
                cancellationToken.ThrowIfCancellationRequested();
                // New-VHD -SizeBytes <size>MB -Path c:\temp\<name>.vhd -Dynamic -Confirm:$false
                using (var cmd = powerShellSession.AddCommand("Initialize-Disk"))
                {
                    cmd.AddParameter("Number", diskNumber);
                    cmd.AddParameter("PartitionStyle", "GPT");
                    cmd.AddParameter("Passthru");
                    var result = await powerShellSession.InvokeAsync(new ProgressNumberOnly(progressReporter, "Initializing virtual hard disk...")).ConfigureAwait(false);

                    cancellationToken.ThrowIfCancellationRequested();

                    if (!result.Any())
                    {
                        throw new InvalidOperationException();
                    }

                    initializedDiskNumber = uint.Parse(result[0].Members["Number"].Value.ToString());
                }
            }

            progressReporter?.Report(new ProgressData(100, "Initializing virtual hard disk..."));
            return initializedDiskNumber;
        }

        private struct MountVhdData
        {
            public MountVhdData(Guid guid, uint diskNumber)
            {
                Guid = guid;
                DiskNumber = diskNumber;
            }

            public Guid Guid;
            public uint DiskNumber;
        }


        private async Task<MountVhdData> MountVhd(string path, CancellationToken cancellationToken, IProgress<ProgressData> progressReporter)
        {
            uint diskNumber;
            Guid guid;

            progressReporter?.Report(new ProgressData(0, "Mounting virtual hard disk..."));

            using (var powerShellSession = await PowerShellSession.CreateForModule("Hyper-V", true).ConfigureAwait(false))
            {
                cancellationToken.ThrowIfCancellationRequested();
                // New-VHD -SizeBytes <size>MB -Path c:\temp\<name>.vhd -Dynamic -Confirm:$false
                using (var cmd = powerShellSession.AddCommand("Mount-VHD"))
                {
                    cmd.AddParameter("Path", path);
                    cmd.AddParameter("Passthru");
                    var result = await powerShellSession.InvokeAsync(new ProgressNumberOnly(progressReporter, "Mounting virtual hard disk...")).ConfigureAwait(false);

                    cancellationToken.ThrowIfCancellationRequested();

                    if (!result.Any())
                    {
                        throw new InvalidOperationException();
                    }

                    guid = Guid.Parse(result[0].Members["DiskIdentifier"].Value.ToString());
                    diskNumber = uint.Parse(result[0].Members["DiskNumber"].Value.ToString());
                }
            }

            progressReporter?.Report(new ProgressData(100, "Mounting virtual hard disk..."));
            return new MountVhdData(guid, diskNumber);
        }

        private async Task DismountVhd(uint diskNumber, CancellationToken cancellationToken, IProgress<ProgressData> progressReporter)
        {
            progressReporter?.Report(new ProgressData(0, "Mounting virtual hard disk..."));

            using (var powerShellSession = await PowerShellSession.CreateForModule("Hyper-V", true).ConfigureAwait(false))
            {
                cancellationToken.ThrowIfCancellationRequested();
                // New-VHD -SizeBytes <size>MB -Path c:\temp\<name>.vhd -Dynamic -Confirm:$false
                using (var cmd = powerShellSession.AddCommand("Dismount-VHD"))
                {
                    cmd.AddParameter("DiskNumber", diskNumber);
                    cmd.AddParameter("Passthru");
                    await powerShellSession.InvokeAsync(new ProgressNumberOnly(progressReporter, "Mounting virtual hard disk...")).ConfigureAwait(false);
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }

            progressReporter?.Report(new ProgressData(100, "Mounting virtual hard disk..."));
        }

        private async Task<string> ExpandMsix(string msixPath, string vhdPath, CancellationToken cancellationToken, IProgress<ProgressData> progressReporter)
        {
            progressReporter?.Report(new ProgressData(0, "Expanding MSIX..."));
            cancellationToken.ThrowIfCancellationRequested();
            await this.MsixMgr.Unpack(msixPath, vhdPath, cancellationToken, progressReporter).ConfigureAwait(false);
            progressReporter?.Report(new ProgressData(100, "Expanding MSIX..."));

            var dir = new DirectoryInfo(vhdPath);
            return dir.EnumerateDirectories().First().Name;
        }

        private async Task CreateScripts(string vhdPath, string packageName, string packageParentFolder, Guid volumeGuid, CancellationToken cancellationToken, IProgress<ProgressData> progressReporter)
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

            var contentStage = await File.ReadAllTextAsync(templateStage, Encoding.UTF8, cancellationToken).ConfigureAwait(false);
            var contentRegister = await File.ReadAllTextAsync(templateRegister, Encoding.UTF8, cancellationToken).ConfigureAwait(false);
            var contentDeregister = await File.ReadAllTextAsync(templateDeregister, Encoding.UTF8, cancellationToken).ConfigureAwait(false);
            var contentDestage = await File.ReadAllTextAsync(templateDestage, Encoding.UTF8, cancellationToken).ConfigureAwait(false);

            // Stage
            // "<path to vhd>"
            // "<package name>"
            // "<package parent folder>"
            // "<vol guid>"

            var vhdDir = Path.GetDirectoryName(vhdPath);
            var vhdFileName = Path.GetFileName(vhdPath);

            contentStage =
                contentStage.Replace("<path to vhd>", vhdPath.Replace("\"", "`\""))
                    .Replace("<package parent folder>", packageParentFolder.Replace("\"", "`\""))
                    .Replace("<package name>", packageName.Replace("\"", "`\""))
                    .Replace("<vol guid>", volumeGuid.ToString("B"));

            contentStage =
                contentStage.Replace("<path to vhd>", vhdDir.Replace("\"", "`\""))
                    .Replace("<package parent folder>", packageParentFolder.Replace("\"", "`\""))
                    .Replace("<package name>", packageName.Replace("\"", "`\""))
                    .Replace("<vol guid>", volumeGuid.ToString("B"));

            contentRegister =
                contentRegister.Replace("<package name>", packageName.Replace("\"", "`\""));

            contentDeregister =
                contentDeregister.Replace("<package name>", packageName.Replace("\"", "`\""));

            contentDestage =
                contentDestage.Replace("<package name>", packageName.Replace("\"", "`\""));

            var baseDir = Path.Combine(vhdDir, vhdFileName + ".Scripts");
            if (!Directory.Exists(baseDir))
            {
                Directory.CreateDirectory(baseDir);
            }

            await File.WriteAllTextAsync(Path.Combine(baseDir, "app-attach-stage.ps1"), contentStage, Encoding.UTF8, cancellationToken).ConfigureAwait(false);
            await File.WriteAllTextAsync(Path.Combine(baseDir, "app-attach-register.ps1"), contentRegister, Encoding.UTF8, cancellationToken).ConfigureAwait(false);
            await File.WriteAllTextAsync(Path.Combine(baseDir, "app-attach-deregister.ps1"), contentDeregister, Encoding.UTF8, cancellationToken).ConfigureAwait(false);
            await File.WriteAllTextAsync(Path.Combine(baseDir, "app-attach-destage.ps1"), contentDestage, Encoding.UTF8, cancellationToken).ConfigureAwait(false);
        }

        private async Task NewVhd(int sizeInMb, string path, CancellationToken cancellationToken, IProgress<ProgressData> progressReporter)
        {
            Logger.Debug("Creating virtual hard dick with size {0}MB in {1}...", sizeInMb, path);
            progressReporter?.Report(new ProgressData(0, "Creating virtual hard disk..."));

            using (var powerShellSession = await PowerShellSession.CreateForModule("Hyper-V", true).ConfigureAwait(false))
            {
                cancellationToken.ThrowIfCancellationRequested();
                // New-VHD -SizeBytes <size>MB -Path c:\temp\<name>.vhd -Dynamic -Confirm:$false
                using (var cmd = powerShellSession.AddCommand("New-VHD"))
                {
                    var sizeBytesString = sizeInMb + "MB";
                    cmd.AddParameter("SizeBytes", sizeBytesString);
                    cmd.AddParameter("Path", path);
                    cmd.AddParameter("Dynamic");
                    cmd.AddParameter("Confirm", false);
                    await powerShellSession.InvokeAsync(new ProgressNumberOnly(progressReporter, "Creating virtual hard disk...")).ConfigureAwait(false);
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }
        }

        private Task StartService(CancellationToken cancellationToken, IProgress<ProgressData> progressReporter)
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

        private Task<bool> StopService(CancellationToken cancellationToken, IProgress<ProgressData> progressReporter)
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


        private async Task<long> GetVhdSize(string packagePath, double extraMargin = 0.2, CancellationToken cancellationToken = default, IProgress<ProgressData> progressReporter = null)
        {
            var total = 0L;
            using (ZipArchive archive = ZipFile.OpenRead(packagePath))
            {
                foreach (var item in archive.Entries)
                {
                    total += item.Length;
                }
            }

            return (long)(total * (1 + extraMargin));

            progressReporter?.Report(new ProgressData(0, "Calculating required size..."));
            var tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            try
            {
                await this.MsixSdk.UnpackPackage(packagePath, tempDirectory, cancellationToken, new ProgressNumberOnly(progressReporter, "Calculating required size...")).ConfigureAwait(false);

                var queue = new Queue<string>();
                queue.Enqueue(tempDirectory);

                var totalSize = 0L;

                while (queue.Any())
                {
                    var take = queue.Dequeue();
                    var dirInfo = new DirectoryInfo(take);
                    if (!dirInfo.Exists)
                    {
                        continue;
                    }

                    foreach (var file in dirInfo.EnumerateFiles("*", SearchOption.TopDirectoryOnly))
                    {
                        totalSize += file.Length;
                    }

                    foreach (var directory in dirInfo.EnumerateDirectories("*", SearchOption.TopDirectoryOnly))
                    {
                        queue.Enqueue(directory.FullName);
                    }
                }

                return (long)(totalSize * (1 + extraMargin));
            }
            finally
            {
                if (Directory.Exists(tempDirectory))
                {
                    Directory.Delete(tempDirectory, true);
                }
            }
        }

        private class ProgressNumberOnly : IProgress<ProgressData>
        {
            private readonly IProgress<ProgressData> parent;
            private readonly string message;

            public ProgressNumberOnly(IProgress<ProgressData> parent, string message)
            {
                this.parent = parent;
                this.message = message;
            }

            public void Report(ProgressData value)
            {
                this.parent?.Report(new ProgressData(value.Progress, this.message));
            }
        }
    }
}
