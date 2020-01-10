using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.Infrastructure.Logging;
using otor.msixhero.lib.Infrastructure.Progress;

namespace otor.msixhero.lib.Infrastructure.Interop
{
    public class MsixSdkWrapper
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(MsixSdkWrapper));

        public async Task SignPackageWithPfx(string filePath, string algorithmType, string pfxPath, string password, string timestampUrl, CancellationToken cancellationToken = default)
        {
            var remove = -1;
            var removeLength = 0;

            var signToolArguments = new StringBuilder();

            signToolArguments.Append("sign");
            signToolArguments.AppendFormat(" /fd {0}", algorithmType);
            signToolArguments.AppendFormat(" /a /f \"{0}\"", pfxPath);

            if (!string.IsNullOrEmpty(password))
            {
                signToolArguments.Append(" /p \"");
                remove = signToolArguments.Length;
                signToolArguments.Append(password);
                removeLength = signToolArguments.Length - remove;
                signToolArguments.Append('"');
            }

            if (!string.IsNullOrEmpty(timestampUrl))
            {
                signToolArguments.AppendFormat(" /tr \"{0}\"", timestampUrl);
            }

            signToolArguments.AppendFormat(" \"{0}\"", filePath);

            var args = signToolArguments.ToString();
            var maskedArgs = remove < 0 ? args : args.Remove(remove, removeLength).Insert(remove, "<removed-from-log>");

            var signTool = GetSdkPath("signTool.exe");
            Logger.Info("Executing {0} {1}", signTool, maskedArgs);

            try
            {
                await RunAsync(signTool, args, cancellationToken, 0).ConfigureAwait(false);
            }
            catch (ProcessWrapperException e)
            {
                var line = e.StandardError.FirstOrDefault(l => l.StartsWith("SignTool Error: "));
                if (line != null)
                {
                    throw new InvalidOperationException($"The package could not be signed (exit code = 0x{e.ExitCode:X2}). {line.Substring("SignTool Error: ".Length)}");
                }

                throw;
            }
        }
        public async Task SignPackageWithPersonal(string filePath, string algorithmType, string thumbprint, bool useMachineStore, string timestampUrl, CancellationToken cancellationToken = default)
        {
            var signToolArguments = new StringBuilder();
            
            signToolArguments.Append("sign");
            signToolArguments.AppendFormat(" /fd {0}", algorithmType);

            if (useMachineStore)
            {
                signToolArguments.Append(" /sm");
            }

            if (!string.IsNullOrEmpty(timestampUrl))
            {
                signToolArguments.AppendFormat(" /tr \"{0}\"", timestampUrl);
            }

            signToolArguments.Append(" /a /s MY ");
            signToolArguments.AppendFormat(" /sha1 \"{0}\"", thumbprint);
            signToolArguments.AppendFormat(" \"{0}\"", filePath);

            var args = signToolArguments.ToString();
            var signTool = GetSdkPath("signTool.exe");
            Logger.Info("Executing {0} {1}", signTool, args);

            try
            {
                await RunAsync(signTool, args, cancellationToken, 0).ConfigureAwait(false);
            }
            catch (ProcessWrapperException e)
            {
                var line = e.StandardError.FirstOrDefault(l => l.StartsWith("SignTool Error: "));
                if (line != null)
                {
                    throw new InvalidOperationException($"The package could not be signed (exit code = 0x{e.ExitCode:X2}). {line.Substring("SignTool Error: ".Length)}");
                }

                throw;
            }
        }

        public Task UnpackPackage(string packageFilePath, string unpackedDirectory, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            var arguments = $"unpack /d \"{unpackedDirectory}\" /p \"{packageFilePath}\" /o";
            return this.RunMakeAppx(arguments, cancellationToken, progress);
        }

        public Task PackPackage(string unpackedDirectory, string packageFilePath, bool compress, CancellationToken cancellationToken, IProgress<ProgressData> progress = null)
        {
            var arguments = $"pack /d \"{unpackedDirectory}\" /p \"{packageFilePath}\" /o";
            if (!compress)
            {
                arguments += " /nc";
            }

            return this.RunMakeAppx(arguments, cancellationToken, progress);
        }

        public static string GetSdkPath(string localName)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "redistr", "sdk", IntPtr.Size == 4 ? "x86" : "x64", localName);
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Could not locale SDK part {path}.", path);
            }

            return path;
        }

        private static async Task<int> RunAsync(string path, string arguments, CancellationToken cancellationToken, params int[] properExitCodes)
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

        private async Task RunMakeAppx(string arguments, CancellationToken cancellationToken, IProgress<ProgressData> progress = null)
        {
            var makeAppx = GetSdkPath("makeappx.exe");
            Logger.Info("Executing {0} {1}", makeAppx, arguments);

            try
            {
                await RunAsync(makeAppx, arguments, cancellationToken, 0).ConfigureAwait(false);
            }
            catch (ProcessWrapperException e)
            {
                var findSimilar = e.StandardError.FirstOrDefault(item => item.StartsWith("MakeAppx : error: Error info: error ", StringComparison.OrdinalIgnoreCase));
                if (findSimilar != null)
                {
                    findSimilar = findSimilar.Substring("MakeAppx : error: Error info: error ".Length);

                    var error = Regex.Match(findSimilar, "([0-9a-zA-Z]+): ");
                    if (error.Success)
                    {
                        findSimilar = findSimilar.Substring(error.Length).Trim();
                        throw new InvalidOperationException($"MakeAppx.exe returned exit code {e.ExitCode} due to error {error.Groups[1].Value}. {findSimilar}");
                    }

                    throw new InvalidOperationException($"MakeAppx.exe returned exit code {e.ExitCode}. {findSimilar}");
                }

                findSimilar = e.StandardError.FirstOrDefault(item => item.StartsWith("MakeAppx : error: 0x", StringComparison.OrdinalIgnoreCase));
                if (findSimilar != null)
                {
                    findSimilar = findSimilar.Substring("MakeAppx : error: ".Length);

                    var error = Regex.Match(findSimilar, "([0-9a-zA-Z]+) \\- ");
                    if (error.Success)
                    {
                        findSimilar = findSimilar.Substring(error.Length).Trim();
                        throw new InvalidOperationException($"MakeAppx.exe returned exit code {e.ExitCode} due to error 0x{error.Groups[1].Value}. {findSimilar}");
                    }

                    throw new InvalidOperationException($"MakeAppx.exe returned exit code {e.ExitCode}. {findSimilar}");
                }

                throw;
            }
        }
    }
}
