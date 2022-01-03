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
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Infrastructure.Logging;
using Otor.MsixHero.Infrastructure.ThirdParty.Exceptions;

namespace Otor.MsixHero.Infrastructure.ThirdParty.Sdk
{
    public class SignToolWrapper : ExeWrapper
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(SignToolWrapper));

        public async Task SignPackageWithDeviceGuard(IEnumerable<string> filePaths, string algorithmType, string dgssTokenPath, string timestampUrl, CancellationToken cancellationToken = default)
        {
            var signToolArguments = new StringBuilder();

            signToolArguments.Append("sign");
            signToolArguments.AppendFormat(" /debug /fd {0}", algorithmType);
            
            if (!string.IsNullOrEmpty(timestampUrl))
            {
                signToolArguments.AppendFormat(" /tr \"{0}\"", timestampUrl);
            }

            var libPath = SdkPathHelper.GetSdkPath("Microsoft.Acs.Dlib.dll");

            signToolArguments.AppendFormat(" /dlib \"{0}\"", libPath);
            signToolArguments.AppendFormat(" /dmdf \"{0}\"", dgssTokenPath);

            foreach (var filePath in filePaths)
            {
                signToolArguments.AppendFormat(" \"{0}\"", filePath);
            }

            var args = signToolArguments.ToString();
            var signTool = SdkPathHelper.GetSdkPath("signTool.exe", BundleHelper.SdkPath);
            Logger.Info("Executing {0} {1}", signTool, args);

            Action<string> callBack = _ => { };

            try
            {
                await RunAsync(signTool, args, cancellationToken, callBack, 0).ConfigureAwait(false);
            }
            catch (ProcessWrapperException e)
            {
                foreach (var err in e.StandardOutput)
                {
                    if (err.IndexOf("0x80192ee7", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        err.IndexOf("System.Net.WebException", StringComparison.OrdinalIgnoreCase) >= 0 &&
                        err.IndexOf("microsoft.com", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        throw new WebException("Unable to reach the Device Guard Signing Service", e);
                    }

                    if (err.IndexOf("0x80190191", StringComparison.OrdinalIgnoreCase) >= 0 || err.IndexOf("System.Net.Http.HttpRequestException", StringComparison.OrdinalIgnoreCase) >= 0 && err.Contains("401"))
                    {
                        throw new UnauthorizedAccessException("The provided account is not authorized to sign via the Device Guard Signing Service", e);
                    }

                    if (err.IndexOf("0x8007000d", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        throw new ArgumentException("The provided JSON token file is invalid", e);
                    }
                }

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

            var signTool = SdkPathHelper.GetSdkPath("signTool.exe", BundleHelper.SdkPath);
            Logger.Info("Executing {0} {1}", signTool, maskedArgs);

            Action<string> callBack = _ => { };

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
            var signTool = SdkPathHelper.GetSdkPath("signTool.exe", BundleHelper.SdkPath);
            Logger.Info("Executing {0} {1}", signTool, args);

            Action<string> callBack = _ => { };

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
                        throw new SdkException($"The package could not be signed (exit code {e.ExitCode}). {specialError}", e.ExitCode);
                    }

                    throw new SdkException($"The package could not be signed (exit code {e.ExitCode}). {line.Substring("SignTool Error: ".Length)}", e.ExitCode);
                }

                if (e.ExitCode != 0)
                {
                    throw new SdkException(e.Message, e.ExitCode, e);
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

            if (lines.Any(l => l.Contains("-2146869243/")))
            {
                error = "Selected timestamp server could not be used to sign the package (error 0x80096005).";
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

            if (parsed.Count > 1)
            {
                if (parsed[^1].Item1 == 0)
                {
                    switch (parsed[^1].Item2?.ToLowerInvariant())
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
                            error = "The selected certificate does not meet the " + parsed[^1].Item2 + " filter.";
                            break;
                    }
                }
            }

            if (error != null)
            {
                Logger.Info("Additional info from SignTool.exe: " + string.Join(Environment.NewLine, lines.Where(l => !string.IsNullOrWhiteSpace(l))));
                return true;
            }

            return false;
        }
    }
}