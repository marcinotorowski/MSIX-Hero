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
    public class MsixMgrWrapper
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(MsixMgrWrapper));
        
        public Task Unpack(string packageFilePath, string unpackedDirectory, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            var arguments = $"-Unpack -packagePath \"{packageFilePath}\" -destination \"{unpackedDirectory}\"";
            return this.RunMsixMgr(arguments, cancellationToken);
        }

        public Task ApplyAcls(string unpackedPackageDirectory, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            var arguments = $"-ApplyACLs -packagePath \"{unpackedPackageDirectory}\"";
            return this.RunMsixMgr(arguments, cancellationToken);
        }

        public static string GetMsixMgrPath(string localName, string baseDirectory = null)
        {
            var baseDir = baseDirectory ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "redistr", "msixmgr");
            var path = Path.Combine(baseDir, IntPtr.Size == 4 ? "x86" : "x64", localName);
            if (!File.Exists(path))
            {
                path = Path.Combine(baseDir, localName);
                if (!File.Exists(path))
                {
                    throw new FileNotFoundException($"Could not locale MSIXMGR part {path}.", path);
                }
            }

            return path;
        }

        private static async Task<int> RunAsync(string path, string arguments, CancellationToken cancellationToken, Action<string> callBack, params int[] properExitCodes)
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

                if (properExitCodes != null && !properExitCodes.Contains(result))
                {
                    throw new ProcessWrapperException($"Process existed with an improper exit code {result}.", result, standardError.Any() ? standardError : standardOutput);
                }

                return result;
            }
        }

        private async Task RunMsixMgr(string arguments, CancellationToken cancellationToken, Action<string> callBack = null)
        {
            var msixmgr = GetMsixMgrPath("msixmgr.exe", BundleHelper.MsixMgrPath);
            Logger.Info("Executing {0} {1}", msixmgr, arguments);

            try
            {
                await RunAsync(msixmgr, arguments, cancellationToken, callBack, 0).ConfigureAwait(false);
            }
            catch (ProcessWrapperException e)
            {
                if (e.ExitCode == -1951596541)
                {
                    throw new InvalidOperationException("Could not expand MSIX Package to the VHD file. The maximum size of the virtual disk is smaller than the file size of expanded MSIX package. Try using a bigger disk size.", e);
                }

                throw new InvalidOperationException($"Expanding of MSIX package to a VHD image failed with error code {e.ExitCode:X2}", e);
            }
        }
    }
}
