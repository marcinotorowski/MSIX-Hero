// MSIX Hero
// Copyright (C) 2021 Marcin Otorowski
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
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Infrastructure.Logging;
using Otor.MsixHero.Infrastructure.ThirdParty.Exceptions;

namespace Otor.MsixHero.Infrastructure.ThirdParty.Sdk
{
    public abstract class ExeWrapper
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ExeWrapper));

        protected static async Task<int> RunAsync(string path, string arguments, CancellationToken cancellationToken, Action<string> callBack, params int[] properExitCodes)
        {
            Logger.Debug("Executing " + path + " " + arguments);
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

                Logger.Trace("Standard error: " + string.Join(Environment.NewLine, standardError));
                Logger.Trace("Standard output: " + string.Join(Environment.NewLine, standardOutput));
                tcs.TrySetResult(process.ExitCode);
            };

            using (cancellationToken.Register(
                () => {
                    tcs.TrySetCanceled();
                    try
                    {
                        if (!process.HasExited)
                        {
                            Logger.Info("Killing the process " + process.Id);
                            process.Kill();
                        }
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
                    throw new ProcessWrapperException(
                        $"Process existed with an improper exit code {result}.", result, 
                        standardError.Any() ? standardError : standardOutput,
                        standardOutput);
                }

                return result;
            }
        }

    }
}