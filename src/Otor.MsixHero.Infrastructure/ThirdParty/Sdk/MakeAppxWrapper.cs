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
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Dapplo.Log;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.ThirdParty.Exceptions;

namespace Otor.MsixHero.Infrastructure.ThirdParty.Sdk
{
    public class MakeAppxWrapper : ExeWrapper
    {
        private static readonly LogSource Logger = new();        
        public Task UnpackPackage(string sourceMsixPath, string unpackedDirectory, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            return this.UnpackPackage(sourceMsixPath, unpackedDirectory, false, cancellationToken);
        }

        public Task UnpackPackage(string sourceMsixPath, string unpackedDirectory, bool validate, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            var wrapper = new PackUnPackProgressWrapper(progress);
            var arguments = $"unpack /d \"{unpackedDirectory}\" /p \"{sourceMsixPath}\" /v /o";
            if (!validate)
            {
                arguments += " /nv";
            }

            return this.RunMakeAppx(arguments, cancellationToken, wrapper.Callback);
        }

        public Task PackPackageDirectory(string unpackedDirectory, string targetMsixPath, bool compress, bool validate, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            var arguments = $"pack /d \"{unpackedDirectory}\" /p \"{targetMsixPath}\" /v /o";
            if (!compress)
            {
                arguments += " /nc";
            }

            if (!validate)
            {
                arguments += " /nv";
            }

            var wrapper = new PackUnPackProgressWrapper(progress);
            return this.RunMakeAppx(arguments, cancellationToken, wrapper.Callback);
        }

        public Task PackPackageFiles(string mappingFile, string targetMsixPath, bool compress, bool validate, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            var arguments = $"pack /f \"{mappingFile}\" /p \"{targetMsixPath}\" /v /o";
            if (!compress)
            {
                arguments += " /nc";
            }

            if (!validate)
            {
                arguments += " /nv";
            }

            var wrapper = new PackUnPackProgressWrapper(progress);
            return this.RunMakeAppx(arguments, cancellationToken, wrapper.Callback);
        }

        private async Task RunMakeAppx(string arguments, CancellationToken cancellationToken, Action<string> callBack = null)
        {
            var makeAppx = SdkPathHelper.GetSdkPath("makeappx.exe", BundleHelper.SdkPath);
            Logger.Info().WriteLine("Executing {0} {1}", makeAppx, arguments);

            try
            {
                await RunAsync(makeAppx, arguments, cancellationToken, callBack, 0).ConfigureAwait(false);
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
                        throw new SdkException($"MakeAppx.exe returned exit code {e.ExitCode} due to error {error.Groups[1].Value}. {findSimilar}", e.ExitCode);
                    }

                    throw new SdkException($"MakeAppx.exe returned exit code {e.ExitCode}. {findSimilar}", e.ExitCode);
                }
                
                findSimilar = e.StandardError.FirstOrDefault(item => item.StartsWith("MakeAppx : error: 0x", StringComparison.OrdinalIgnoreCase));
                if (findSimilar != null)
                {
                    var manifestError = e.StandardError.FirstOrDefault(item => item.StartsWith("MakeAppx : error: Manifest validation error: "));
                    manifestError = manifestError?.Substring("MakeAppx : error: Manifest validation error: ".Length);

                    findSimilar = findSimilar.Substring("MakeAppx : error: ".Length);

                    int exitCode;
                    var error = Regex.Match(findSimilar, "([0-9a-zA-Z]+) \\- ");
                    if (error.Success)
                    {
                        if (!string.IsNullOrEmpty(manifestError))
                        {
                            findSimilar = manifestError;
                        }
                        else
                        {
                            findSimilar = findSimilar.Substring(error.Length).Trim();
                        }
                        
                        if (int.TryParse(error.Groups[1].Value, out exitCode) && exitCode > 0)
                        {
                            throw new SdkException($"MakeAppx.exe returned exit code {e.ExitCode} due to error {error.Groups[1].Value}. {findSimilar}", exitCode);
                        }

                        if (error.Groups[1].Value.StartsWith("0x", StringComparison.Ordinal))
                        {
                            exitCode = Convert.ToInt32(error.Groups[1].Value, 16);
                            if (exitCode != 0)
                            {
                                throw new SdkException($"MakeAppx.exe returned exit code {e.ExitCode} due to error {error.Groups[1].Value}. {findSimilar}", exitCode);
                            }
                        }

                        throw new InvalidOperationException($"MakeAppx.exe returned exit code {e.ExitCode} due to error {error.Groups[1].Value}. {findSimilar}");
                    }

                    if (!string.IsNullOrEmpty(manifestError))
                    {
                        findSimilar = manifestError;
                    }
                    
                    if (int.TryParse(error.Groups[1].Value, out exitCode) && exitCode > 0)
                    {
                        throw new SdkException($"MakeAppx.exe returned exit code {e.ExitCode}. {findSimilar}", exitCode);
                    }

                    if (error.Groups[1].Value.StartsWith("0x", StringComparison.Ordinal))
                    {
                        exitCode = Convert.ToInt32(error.Groups[1].Value, 16);
                        if (exitCode != 0)
                        {
                            throw new SdkException($"MakeAppx.exe returned exit code {e.ExitCode}. {findSimilar}", exitCode);
                        }
                    }

                    throw new SdkException($"MakeAppx.exe returned exit code {e.ExitCode}. {findSimilar}", e.ExitCode);
                }

                throw;
            }
        }
        
        private class PackUnPackProgressWrapper
        {
            private readonly IProgress<ProgressData> progressReporter;
            
            private int? fileCounter;

            private int alreadyProcessed;

            public PackUnPackProgressWrapper(IProgress<ProgressData> progressReporter)
            {
                this.progressReporter = progressReporter;
            }

            public Action<string> Callback => this.OnProgress;

            private void OnProgress(string data)
            {
                if (string.IsNullOrEmpty(data) || this.progressReporter == null)
                {
                    return;
                }

                if (!this.fileCounter.HasValue)
                {
                    var match = Regex.Match(data, @"^Packing (\d+) files?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                    if (match.Success)
                    {
                        this.fileCounter = int.Parse(match.Groups[1].Value);
                    }
                }

                var regexFile = Regex.Match(data, "^Processing \"([^\"]+)\"", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                if (regexFile.Success)
                {
                    this.alreadyProcessed++;
                    int currentProgress;
                    if (this.fileCounter.HasValue && this.fileCounter.Value > 0)
                    {
                        currentProgress = (int)(100.0 * this.alreadyProcessed / this.fileCounter.Value);
                    }
                    else
                    {
                        currentProgress = 0;
                    }

                    var fileName = regexFile.Groups[1].Value;
                    if (string.IsNullOrEmpty(fileName))
                    {
                        return;
                    }

                    fileName = Path.GetFileName(fileName);
                    this.progressReporter.Report(new ProgressData(currentProgress, $"Compressing {fileName}..."));
                }
                else
                {
                    regexFile = Regex.Match(data, "^Extracting file ([^ ]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                    if (regexFile.Success)
                    {
                        this.alreadyProcessed++;
                        int currentProgress;
                        if (this.fileCounter.HasValue && this.fileCounter.Value > 0)
                        {
                            currentProgress = (int)(100.0 * this.alreadyProcessed / this.fileCounter.Value);
                        }
                        else
                        {
                            currentProgress = 0;
                        }

                        var fileName = regexFile.Groups[1].Value;
                        if (string.IsNullOrEmpty(fileName))
                        {
                            return;
                        }

                        fileName = Path.GetFileName(fileName);
                        this.progressReporter.Report(new ProgressData(currentProgress, $"Extracting {fileName}..."));
                    }
                }
            }
        }
    }
}
