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
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapplo.Log;
using Otor.MsixHero.Infrastructure.ThirdParty.Exceptions;

namespace Otor.MsixHero.Infrastructure.ThirdParty.Sdk
{
    public abstract class ExeWrapper
    {
        private static readonly LogSource Logger = new();
        protected static Task<int> RunAsync(string path, string arguments, CancellationToken cancellationToken, Action<string> callBack, params int[] properExitCodes)
        {
            return RunAsync(path, arguments, null, cancellationToken, callBack, properExitCodes);
        }

        protected static async Task<int> RunAsync(string path, string arguments, string workingDirectory, CancellationToken cancellationToken, Action<string> callBack, params int[] properExitCodes)
        {
            Logger.Debug().WriteLine("Executing " + path + " " + arguments);
            var processStartInfo = new ProcessStartInfo(path, arguments);

            var standardOutput = new List<string>();
            var standardError = new List<string>();

            // force some settings in the start info so we can capture the output
            processStartInfo.UseShellExecute = false;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardError = true;
            processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            processStartInfo.CreateNoWindow = true;

            if (workingDirectory != null)
            {
                processStartInfo.WorkingDirectory = workingDirectory;
            }
            
            var tcs = new TaskCompletionSource<int>();

            var process = new Process
            {
                StartInfo = processStartInfo,
                EnableRaisingEvents = true
            };

            var standardOutputResults = new TaskCompletionSource<string[]>();
            process.OutputDataReceived += (_, args) =>
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
            process.ErrorDataReceived += (_, args) =>
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
            
            process.Exited += async (_, _) =>
            {
                await standardOutputResults.Task.ConfigureAwait(false);
                await standardErrorResults.Task.ConfigureAwait(false);

                Logger.Verbose().WriteLine("Standard error: " + string.Join(Environment.NewLine, standardError));
                Logger.Verbose().WriteLine("Standard output: " + string.Join(Environment.NewLine, standardOutput));
                tcs.TrySetResult(process.ExitCode);
            };

            await using (cancellationToken.Register(
                () => {
                    tcs.TrySetCanceled();
                    try
                    {
                        if (!process.HasExited)
                        {
                            Logger.Info().WriteLine("Killing the process " + process.Id);
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

                if (standardOutput.Any())
                {
                    Logger.Debug().WriteLine("Process has finished and returned the following standard output:\r\n" + string.Join(Environment.NewLine, standardOutput));
                }
                else
                {
                    Logger.Debug().WriteLine("Process has finished and did not return anything to standard output.");
                }

                if (standardError.Any())
                {
                    Logger.Debug().WriteLine("Process has finished and returned the following standard error:\r\n" + string.Join(Environment.NewLine, standardError));
                }
                else
                {
                    Logger.Debug().WriteLine("Process has finished and did not return anything to standard error.");
                }

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