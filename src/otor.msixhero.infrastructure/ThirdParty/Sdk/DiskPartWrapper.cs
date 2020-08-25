using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Infrastructure.Logging;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.ThirdParty.Exceptions;

namespace Otor.MsixHero.Infrastructure.ThirdParty.Sdk
{
    public class DiskPartWrapper
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

        public static string GetDiskPartPath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "diskpart.exe");
        }

        private static async Task RunAsync(string path, string arguments, CancellationToken cancellationToken, Action<string> callBack, params int[] properExitCodes)
        {
            var processStartInfo = new ProcessStartInfo(path, arguments);
            
            var standardOutput = new List<string>();
            var standardError = new List<string>();

            // force some settings in the start info so we can capture the output
            processStartInfo.UseShellExecute = false;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardError = true;
            processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            processStartInfo.CreateNoWindow = true;

            var tcs = new TaskCompletionSource<int>();

            var process = new Process
            {
                StartInfo = processStartInfo,
                EnableRaisingEvents = true
            };

            var standardOutputResults = new TaskCompletionSource<string[]>();
            process.OutputDataReceived += (sender, args) =>
            {
                callBack?.Invoke(args.Data);
                if (args.Data != null)
                {
                    standardOutput.Add(args.Data);
                }
                else
                {
                    standardOutputResults.SetResult(standardOutput.ToArray());
                }
            };

            var standardErrorResults = new TaskCompletionSource<string[]>();
            process.ErrorDataReceived += (sender, args) =>
            {
                if (args.Data != null)
                {
                    standardError.Add(args.Data);
                }
                else
                {
                    standardErrorResults.SetResult(standardError.ToArray());
                }
            };

            process.Exited += async (sender, args) =>
            {
                await standardOutputResults.Task.ConfigureAwait(false);
                await standardErrorResults.Task.ConfigureAwait(false);

                tcs.TrySetResult(process.ExitCode);
            };

            using (cancellationToken.Register(
                () => {
                    tcs.TrySetCanceled();
                    try
                    {
                        if (!process.HasExited)
                            process.Kill();
                    }
                    catch (InvalidOperationException) { }
                }))
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!process.Start())
                {
                    tcs.TrySetException(new InvalidOperationException("Failed to start process"));
                }
                else
                {
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                }

                var result = await tcs.Task.ConfigureAwait(false);

                if (standardError.Any())
                {
                    Logger.Warn("Standard error goes below...");
                    foreach (var item in standardError)
                    {
                        Logger.Warn(item);
                    }
                }

                if (standardOutput.Any())
                {
                    Logger.Debug("Standard output goes below...");
                    foreach (var item in standardOutput)
                    {
                        Logger.Debug(item);
                    }
                }

                if (properExitCodes != null && !properExitCodes.Contains(result))
                {
                    throw new ProcessWrapperException($"Process existed with an improper exit code {result}.", result, standardError.Any() ? standardError : standardOutput);
                }
            }
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
