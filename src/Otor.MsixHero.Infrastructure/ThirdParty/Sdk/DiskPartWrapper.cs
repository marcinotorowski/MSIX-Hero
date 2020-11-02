using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Infrastructure.Logging;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.ThirdParty.Exceptions;

namespace Otor.MsixHero.Infrastructure.ThirdParty.Sdk
{
    public class DiskPartWrapper : ExeWrapper
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(DiskPartWrapper));
        
        public async Task MountVhd(string vhdPath, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            var tempFile = Path.Combine(Path.GetTempPath(), "msix-hero-vhd-" + Guid.NewGuid().ToString("N").Substring(0, 10) + ".cfg");

            try
            {
                var content = @"select vdisk file = ""{0}""
attach vdisk";

                await File.WriteAllTextAsync(tempFile, string.Format(content, vhdPath), cancellationToken).ConfigureAwait(false);
                var arguments = $"/S \"{tempFile}\"";
                await this.RunDiskPart(arguments, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }
        
        public async Task CreateVhdAndAssignDriveLetter(string vhdPath, long requiredSize, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            var tempFile = Path.Combine(Path.GetTempPath(), "msix-hero-vhd-" + Guid.NewGuid().ToString("N").Substring(0, 10) + ".cfg");

            try
            {
                var content = @"create vdisk file=""{0}"" maximum={1} type=expandable";

                var requiredSizeMb = (int) (10 * Math.Ceiling(0.1 * requiredSize / 1024 / 1024));
                await File.WriteAllTextAsync(tempFile, string.Format(content, vhdPath, requiredSizeMb), cancellationToken).ConfigureAwait(false);
                var arguments = $"/S \"{tempFile}\"";
                await this.RunDiskPart(arguments, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }

            try
            {
                var content = @"select vdisk file = ""{0}""
attach vdisk
create partition primary
format fs=ntfs
assign";

                await File.WriteAllTextAsync(tempFile, string.Format(content, vhdPath), cancellationToken).ConfigureAwait(false);
                var arguments = $"/S \"{tempFile}\"";
                await this.RunDiskPart(arguments, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }
        
        public async Task DismountVhd(string vhdPath, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            var tempFile = Path.Combine(Path.GetTempPath(), "msix-hero-vhd-" + Guid.NewGuid().ToString("N").Substring(0, 10) + ".cfg");

            try
            {
                var content = @"select vdisk file = ""{0}""
detach vdisk";

                await File.WriteAllTextAsync(tempFile, string.Format(content, vhdPath), cancellationToken).ConfigureAwait(false);
                var arguments = $"/S \"{tempFile}\"";
                await this.RunDiskPart(arguments, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        private static string GetDiskPartPath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "diskpart.exe");
        }
        
        private async Task RunDiskPart(string arguments, CancellationToken cancellationToken, Action<string> callBack = null)
        {
            var diskPart = GetDiskPartPath();
            Logger.Info("Executing {0} {1}", diskPart, arguments);

            try
            {
                await RunAsync(diskPart, arguments, cancellationToken, callBack, 0).ConfigureAwait(false);
            }
            catch (ProcessWrapperException e)
            {
                throw new InvalidOperationException($"Diskpart.exe failed with error code {e.ExitCode:X2}", e);
            }
        }
    }
}
