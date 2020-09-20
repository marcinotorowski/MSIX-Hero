using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Infrastructure.ThirdParty.Exceptions;

namespace Otor.MsixHero.Infrastructure.ThirdParty.Sdk
{
    public abstract class ExeWrapper
    {
        protected static async Task<int> RunAsync(string path, string arguments, CancellationToken cancellationToken, Action<string> callBack, params int[] properExitCodes)
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

                if (properExitCodes != null && properExitCodes.Any() && !properExitCodes.Contains(result))
                {
                    throw new ProcessWrapperException($"Process existed with an improper exit code {result}.", result, standardError.Any() ? standardError : standardOutput);
                }

                return result;
            }
        }

    }
}