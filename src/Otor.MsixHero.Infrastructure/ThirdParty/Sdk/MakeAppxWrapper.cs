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
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Dapplo.Log;
using Otor.MsixHero.Infrastructure.Helpers;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.ThirdParty.Exceptions;

namespace Otor.MsixHero.Infrastructure.ThirdParty.Sdk
{
    public class MakeAppxWrapper : ExeWrapper
    {
        private static readonly LogSource Logger = new();

        public Task Unpack(MakeAppxUnpackOptions options, IProgress<ProgressData> progress = default, CancellationToken cancellationToken = default)
        {
            var wrapper = new PackUnPackProgressWrapper(progress);
            var arguments = new StringBuilder("unpack", 256);
            arguments.Append(" /d ");
            arguments.Append(CommandLineHelper.EncodeParameterArgument(options.Target.FullName));

            arguments.Append(" /p ");
            arguments.Append(CommandLineHelper.EncodeParameterArgument(options.Source.FullName));

            if (options.Overwrite)
            {
                arguments.Append(" /o");
            }

            if (options.Verbose)
            {
                arguments.Append(" /v");
            }

            if (!options.Validate)
            {
                arguments.Append(" /nv");
            }

            return this.RunMakeAppx(arguments.ToString(), wrapper.Callback, cancellationToken);
        }
        
        public Task Pack(MakeAppxPackOptions options, IProgress<ProgressData> progress = null, CancellationToken cancellationToken = default)
        {
            var arguments = new StringBuilder("pack", 256);

            if (options.Source is FileInfo fileInfo)
            {
                arguments.Append(" /f ");
                arguments.Append(CommandLineHelper.EncodeParameterArgument(fileInfo.FullName));
            }
            else
            {
                arguments.Append(" /d ");
                arguments.Append(CommandLineHelper.EncodeParameterArgument(options.Source.FullName));
            }

            arguments.Append(" /p ");
            arguments.Append(CommandLineHelper.EncodeParameterArgument(options.Target.FullName));

            if (options.Verbose)
            {
                arguments.Append(" /v");
            }

            if (options.Overwrite)
            {
                arguments.Append(" /o");
            }

            if (!options.Compress)
            {
                arguments.Append(" /nc");
            }

            if (!options.Validate)
            {
                arguments.Append(" /nv");
            }

            if (options.PublisherBridge != null)
            {
                arguments.Append(" /pb ");
                arguments.Append(CommandLineHelper.EncodeParameterArgument(options.PublisherBridge));
            }

            var wrapper = new PackUnPackProgressWrapper(progress);
            return this.RunMakeAppx(arguments.ToString(), wrapper.Callback, cancellationToken);
        }
        
        private async Task RunMakeAppx(string arguments, Action<string> callBack, CancellationToken cancellationToken = default)
        {
            var makeAppx = SdkPathHelper.GetSdkPath("makeappx.exe", BundleHelper.SdkPath);
            Logger.Info().WriteLine(Resources.Localization.Infrastructure_Sdk_Executing_Format, makeAppx, arguments);

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
                        throw new SdkException(string.Format(Resources.Localization.Infrastructure_Sdk_MakeAppx_EditCodeError_Format, e.ExitCode, error.Groups[1].Value) + " " + findSimilar, e.ExitCode);
                    }

                    throw new SdkException(string.Format(Resources.Localization.Infrastructure_Sdk_MakeAppx_EditCode_Format, e.ExitCode) + " " + findSimilar, e.ExitCode);
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
                            throw new SdkException(string.Format(Resources.Localization.Infrastructure_Sdk_MakeAppx_EditCodeError_Format, e.ExitCode, error.Groups[1].Value) + " " + findSimilar, exitCode);
                        }

                        if (error.Groups[1].Value.StartsWith("0x", StringComparison.Ordinal))
                        {
                            exitCode = Convert.ToInt32(error.Groups[1].Value, 16);
                            if (exitCode != 0)
                            {
                                throw new SdkException(string.Format(Resources.Localization.Infrastructure_Sdk_MakeAppx_EditCodeError_Format, e.ExitCode, error.Groups[1].Value) + " " + findSimilar, exitCode);
                            }
                        }

                        throw new SdkException(string.Format(Resources.Localization.Infrastructure_Sdk_MakeAppx_EditCodeError_Format, e.ExitCode, error.Groups[1].Value) + " " + findSimilar, exitCode);
                    }

                    if (!string.IsNullOrEmpty(manifestError))
                    {
                        findSimilar = manifestError;
                    }

                    if (int.TryParse(error.Groups[1].Value, out exitCode) && exitCode > 0)
                    {
                        throw new SdkException(string.Format(Resources.Localization.Infrastructure_Sdk_MakeAppx_EditCode_Format, e.ExitCode) + " " + findSimilar, exitCode);
                    }

                    if (error.Groups[1].Value.StartsWith("0x", StringComparison.Ordinal))
                    {
                        exitCode = Convert.ToInt32(error.Groups[1].Value, 16);
                        if (exitCode != 0)
                        {
                            throw new SdkException(string.Format(Resources.Localization.Infrastructure_Sdk_MakeAppx_EditCode_Format, e.ExitCode) + " " + findSimilar, exitCode);
                        }
                    }

                    throw new SdkException(string.Format(Resources.Localization.Infrastructure_Sdk_MakeAppx_EditCode_Format, e.ExitCode) + " " + findSimilar, exitCode);
                }

                throw;
            }
        }

        private class PackUnPackProgressWrapper
        {
            private readonly IProgress<ProgressData> _progressReporter;
            
            private int? _fileCounter;

            private int _alreadyProcessed;

            public PackUnPackProgressWrapper(IProgress<ProgressData> progressReporter)
            {
                this._progressReporter = progressReporter;
            }

            public Action<string> Callback => this.OnProgress;

            private void OnProgress(string data)
            {
                if (string.IsNullOrEmpty(data) || this._progressReporter == null)
                {
                    return;
                }

                if (!this._fileCounter.HasValue)
                {
                    var match = Regex.Match(data, @"^Packing (\d+) files?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                    if (match.Success)
                    {
                        this._fileCounter = int.Parse(match.Groups[1].Value);
                    }
                }

                var regexFile = Regex.Match(data, "^Processing \"([^\"]+)\"", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                if (regexFile.Success)
                {
                    this._alreadyProcessed++;
                    int currentProgress;
                    if (this._fileCounter.HasValue && this._fileCounter.Value > 0)
                    {
                        currentProgress = (int)(100.0 * this._alreadyProcessed / this._fileCounter.Value);
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
                    this._progressReporter.Report(new ProgressData(currentProgress, $"Compressing {fileName}..."));
                }
                else
                {
                    regexFile = Regex.Match(data, "^Extracting file ([^ ]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                    if (regexFile.Success)
                    {
                        this._alreadyProcessed++;
                        int currentProgress;
                        if (this._fileCounter.HasValue && this._fileCounter.Value > 0)
                        {
                            currentProgress = (int)(100.0 * this._alreadyProcessed / this._fileCounter.Value);
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
                        this._progressReporter.Report(new ProgressData(currentProgress, string.Format(Resources.Localization.Infrastructure_Sdk_MakeAppx_ExtractingFile_Format, fileName)));
                    }
                }
            }
        }
    }
}
