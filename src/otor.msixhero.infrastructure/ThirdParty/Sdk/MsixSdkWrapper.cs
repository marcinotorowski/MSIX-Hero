using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Infrastructure.Logging;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.ThirdParty.Exceptions;

namespace Otor.MsixHero.Infrastructure.ThirdParty.Sdk
{
    public class MsixSdkWrapper : ExeWrapper
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(MsixSdkWrapper));
        
        public async Task SignPackageWithPfx(IEnumerable<string> filePaths, string algorithmType, string pfxPath, string password, string timestampUrl, CancellationToken cancellationToken = default)
        {
            var remove = -1;
            var removeLength = 0;

            var signToolArguments = new StringBuilder();

            signToolArguments.Append("sign");
            signToolArguments.AppendFormat(" /debug /fd {0}", algorithmType);
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

            foreach (var filePath in filePaths)
            {
                signToolArguments.AppendFormat(" \"{0}\"", filePath);
            }

            var args = signToolArguments.ToString();
            var maskedArgs = remove < 0 ? args : args.Remove(remove, removeLength).Insert(remove, "<removed-from-log>");

            var signTool = GetSdkPath("signTool.exe", BundleHelper.SdkPath);
            Logger.Info("Executing {0} {1}", signTool, maskedArgs);

            Action<string> callBack = data => { };

            try
            {
                await RunAsync(signTool, args, cancellationToken,  callBack, 0).ConfigureAwait(false);
            }
            catch (ProcessWrapperException e)
            {
                var line = e.StandardError.FirstOrDefault(l => l.StartsWith("SignTool Error: "));
                if (line != null)
                {
                    if (TryGetErrorMessageFromSignToolOutput(e.StandardOutput, out var specialError))
                    {
                        throw new SdkException($"The package could not be signed (error 0x{e.ExitCode:X2}). {specialError}", e.ExitCode);
                    }

                    throw new SdkException($"The package could not be signed (error = 0x{e.ExitCode:X2}). {line.Substring("SignTool Error: ".Length)}", e.ExitCode);
                }

                if (e.ExitCode != 0)
                {
                    throw new SdkException(e.Message, e.ExitCode, e);
                }

                throw;
            }
        }

        public async Task SignPackageWithPersonal(IEnumerable<string> filePaths, string algorithmType, string thumbprint, bool useMachineStore, string timestampUrl, CancellationToken cancellationToken = default)
        {
            var signToolArguments = new StringBuilder();
            
            signToolArguments.Append("sign");
            signToolArguments.AppendFormat(" /debug /fd {0}", algorithmType);

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

            foreach (var filePath in filePaths)
            {
                signToolArguments.AppendFormat(" \"{0}\"", filePath);
            }

            var args = signToolArguments.ToString();
            var signTool = GetSdkPath("signTool.exe", BundleHelper.SdkPath);
            Logger.Info("Executing {0} {1}", signTool, args);

            Action<string> callBack = data => { };

            try
            {
                await RunAsync(signTool, args, cancellationToken, callBack, 0).ConfigureAwait(false);
            }
            catch (ProcessWrapperException e)
            {
                var line = e.StandardError.FirstOrDefault(l => l.StartsWith("SignTool Error: "));
                if (line != null)
                {
                    if (TryGetErrorMessageFromSignToolOutput(e.StandardOutput, out var specialError))
                    {
                        throw new SdkException($"The package could not be signed (error 0x{e.ExitCode:X2}). {specialError}", e.ExitCode);
                    }

                    throw new SdkException($"The package could not be signed (error 0x{e.ExitCode:X2}). {line.Substring("SignTool Error: ".Length)}", e.ExitCode);
                }

                if (e.ExitCode != 0)
                {
                    throw new SdkException(e.Message, e.ExitCode, e);
                }

                throw;
            }
        }

        public async Task ComparePackages(string file1, string file2, string pathToSaveXml, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            var comparePackage = GetSdkPath("ComparePackage.exe", BundleHelper.SdkPath);
            if (comparePackage == null)
            {
                throw new FileNotFoundException("ComparePackage.exe not found.");
            }

            var arguments = $@"""{file1}"" ""{file2}"" -o -XML ""{pathToSaveXml}""";

            await RunAsync(comparePackage, arguments, cancellationToken, null, 0).ConfigureAwait(false);
        }

        public Task UnpackPackage(string packageFilePath, string unpackedDirectory, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            var wrapper = new PackUnPackProgressWrapper(progress);
            var arguments = $"unpack /d \"{unpackedDirectory}\" /p \"{packageFilePath}\" /v /o";
            return this.RunMakeAppx(arguments, cancellationToken, wrapper.Callback);
        }

        public Task PackPackageDirectory(string unpackedDirectory, string packageFilePath, bool compress, bool validate, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            var arguments = $"pack /d \"{unpackedDirectory}\" /p \"{packageFilePath}\" /v /o";
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

        public Task PackPackageFiles(string mappingFile, string packageFilePath, bool compress, bool validate, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            var arguments = $"pack /f \"{mappingFile}\" /p \"{packageFilePath}\" /v /o";
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

        public static string GetSdkPath(string localName, string baseDirectory = null)
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            var baseDir = baseDirectory ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "redistr", "sdk");
            var path = Path.Combine(baseDir, IntPtr.Size == 4 ? "x86" : "x64", localName);
            if (!File.Exists(path))
            {
                path = Path.Combine(baseDir, localName);
                if (!File.Exists(path))
                {
                    throw new FileNotFoundException($"Could not locate SDK part {path}.", path);
                }
            }

            return path;
        }

        private async Task RunMakeAppx(string arguments, CancellationToken cancellationToken, Action<string> callBack = null)
        {
            var makeAppx = GetSdkPath("makeappx.exe", BundleHelper.SdkPath);
            Logger.Info("Executing {0} {1}", makeAppx, arguments);

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
                    findSimilar = findSimilar.Substring("MakeAppx : error: ".Length);

                    int exitCode;
                    var error = Regex.Match(findSimilar, "([0-9a-zA-Z]+) \\- ");
                    if (error.Success)
                    {
                        findSimilar = findSimilar.Substring(error.Length).Trim();

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

        public static bool TryGetErrorMessageFromSignToolOutput(IList<string> lines, out string error)
        {
            error = null;
            var extended = new StringBuilder();

            var count = 0;

            var parsed = new List<Tuple<int, string>>();

            if (lines.Any(l => l.Contains("-2147024885/0x8007000b")))
            {
                error = "Package publisher and certificate subject are not the same. MSIX app cannot be signed with the selected certificate.";
                return true;
            }

            foreach (var line in lines)
            {
                if (line.Contains("Issued by: "))
                {
                    count++;
                    continue;
                }

                if (!line.StartsWith("After "))
                {
                    continue;
                }

                /*
                    After EKU filter, 2 certs were left.
                    After expiry filter, 2 certs were left.
                    After Private Key filter, 0 certs were left.
                 */
                var match = Regex.Match(line, @"After (.*) filter, (\d+) certs were left.");
                if (!match.Success)
                {
                    continue;
                }

                extended.AppendLine("* " + line.Trim());
                parsed.Add(new Tuple<int, string>(int.Parse(match.Groups[2].Value), match.Groups[1].Value));
            }

            parsed.Insert(0, new Tuple<int, string>(count, null));
            for (var i = parsed.Count - 1; i > 0; i--)
            {
                if (parsed[i].Item1 == parsed[i - 1].Item1)
                {
                    parsed.RemoveAt(i);
                }
            }

            var findHash = parsed.FirstOrDefault(p => p.Item2 == "Hash");
            if (findHash != null && findHash.Item1 == 0)
            {
                var findHashIndex = parsed.LastIndexOf(findHash);
                if (findHashIndex == 2)
                {
                    switch (parsed.Skip(1).First().Item2.ToLowerInvariant())
                    {
                        case "private key":
                            error = "The selected certificate does not have a private key.";
                            break;
                        case "expiry":
                            error = "The selected certificate has expired.";
                            break;
                        case "eku":
                            error = "The selected certificate cannot be used for signing purposes (EKU mismatch).";
                            break;
                        default:
                            error = "The selected certificate does not meet the " + parsed[parsed.Count - 1].Item2 + " filter.";
                            break;
                    }

                    return true;
                }
                else if (findHashIndex > 0)
                {
                    error = "The selected certificate failed validation of one of the following filters: " + string.Join(", ", parsed.Skip(1).Take(findHashIndex - 1).Select(x => x.Item2));
                    return true;
                }
            }

            if (parsed.Count > 0)
            {
                if (parsed[parsed.Count - 1].Item1 == 0)
                {
                    switch (parsed[parsed.Count - 1].Item2.ToLowerInvariant())
                    {
                        case "private key":
                            error = "The selected certificate does not have a private key.";
                            break;
                        case "expiry":
                            error = "The selected certificate has expired.";
                            break;
                        case "eku":
                            error = "The selected certificate cannot be used for signing purposes (EKU mismatch).";
                            break;
                        default:
                            error = "The selected certificate does not meet the " + parsed[parsed.Count - 1].Item2 + " filter.";
                            break;
                    }
                }
            }

            for (var i = 0; i < parsed.Count; i++)
            {
                if (parsed[i].Item1 == 0)
                {

                    break;
                }
            }

            if (error != null)
            {
                Logger.Info("Additional info from SignTool.exe: " + string.Join(Environment.NewLine, lines.Where(l => !string.IsNullOrWhiteSpace(l))));
                return true;
            }

            return false;
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
