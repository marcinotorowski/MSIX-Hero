﻿// MSIX Hero
// Copyright (C) 2024 Marcin Otorowski
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
using Dapplo.Log;
using Otor.MsixHero.Infrastructure.Helpers;
using Otor.MsixHero.Infrastructure.ThirdParty.Exceptions;

namespace Otor.MsixHero.Infrastructure.ThirdParty.Sdk
{
    public class SignToolWrapper : ExeWrapper
    {
        private static readonly LogSource Logger = new();
        public async Task SignWithDeviceGuard(IEnumerable<string> filePaths, string algorithmType, string dgssTokenPath, string timestampUrl, CancellationToken cancellationToken = default)
        {
            var signToolArguments = new StringBuilder(256);

            signToolArguments.Append("sign");
            signToolArguments.Append(" /debug /fd ");
            signToolArguments.Append(CommandLineHelper.EncodeParameterArgument(algorithmType));
            
            if (!string.IsNullOrEmpty(timestampUrl))
            {
                signToolArguments.Append(" /tr ");
                signToolArguments.Append(CommandLineHelper.EncodeParameterArgument(timestampUrl));

                // required in SDK builds 20236 and later
                // see https://docs.microsoft.com/en-us/dotnet/framework/tools/signtool-exe
                signToolArguments.Append(" /td SHA256");
            }

            var libPath = SdkPathHelper.GetSdkPath("Microsoft.Acs.Dlib.dll");

            signToolArguments.Append(" /dlib ");
            signToolArguments.Append(CommandLineHelper.EncodeParameterArgument(libPath));

            signToolArguments.Append(" /dmdf ");
            signToolArguments.Append(CommandLineHelper.EncodeParameterArgument(dgssTokenPath));

            foreach (var filePath in filePaths)
            {
                signToolArguments.Append(' ');
                signToolArguments.Append(CommandLineHelper.EncodeParameterArgument(filePath));
            }

            var args = signToolArguments.ToString();
            var signTool = SdkPathHelper.GetSdkPath("signTool.exe", BundleHelper.SdkPath);
            Logger.Info().WriteLine("Executing {0} {1}", signTool, args);

            Action<string> callBack = _ => { };

            try
            {
                await RunAsync(signTool, args, 0, callBack, cancellationToken).ConfigureAwait(false);
            }
            catch (ProcessWrapperException e)
            {
                foreach (var err in e.StandardOutput)
                {
                    if (err.IndexOf("0x80192ee7", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        err.IndexOf("System.Net.WebException", StringComparison.OrdinalIgnoreCase) >= 0 &&
                        err.IndexOf("microsoft.com", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        throw new WebException(Resources.Localization.Infrastructure_Sdk_Error_DeviceGuardNotReachable, e);
                    }

                    if (err.IndexOf("0x80190191", StringComparison.OrdinalIgnoreCase) >= 0 || err.IndexOf("System.Net.Http.HttpRequestException", StringComparison.OrdinalIgnoreCase) >= 0 && err.Contains("401"))
                    {
                        throw new UnauthorizedAccessException(Resources.Localization.Infrastructure_Sdk_Error_DeviceGuardAccountNotAuthorized, e);
                    }

                    if (err.IndexOf("0x8007000d", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        throw new ArgumentException(Resources.Localization.Infrastructure_Sdk_Error_DeviceGuardInvalidJson, e);
                    }
                }

                var line = e.StandardError.FirstOrDefault(l => l.StartsWith("SignTool Error: "));
                if (line != null)
                {
                    if (TryGetErrorMessageFromSignToolOutput(e.StandardOutput, out var specialError))
                    {
                        throw new SdkException(string.Format(Resources.Localization.Infrastructure_Sdk_Error_SigningError_Format, "0x" + e.ExitCode.ToString("X2")) + " " + specialError, e.ExitCode);
                    }

                    throw new SdkException(string.Format(Resources.Localization.Infrastructure_Sdk_Error_SigningError_Format, "0x" + e.ExitCode.ToString("X2")) + $" {line.Substring("SignTool Error: ".Length)}", e.ExitCode);
                }

                if (e.ExitCode != 0)
                {
                    throw new SdkException(e.Message, e.ExitCode, e);
                }

                throw;
            }
        }

        public async Task SignWithPfx(IEnumerable<string> filePaths, string algorithmType, string pfxPath, string password, string timestampUrl, CancellationToken cancellationToken = default)
        {
            var remove = -1;
            var removeLength = 0;

            var signToolArguments = new StringBuilder(256);

            signToolArguments.Append("sign");
            signToolArguments.Append(" /debug /fd ");
            signToolArguments.Append(CommandLineHelper.EncodeParameterArgument(algorithmType));
            signToolArguments.Append(" /a /f ");
            signToolArguments.Append(CommandLineHelper.EncodeParameterArgument(pfxPath));

            if (!string.IsNullOrEmpty(password))
            {
                signToolArguments.Append(" /p ");
                remove = signToolArguments.Length;
                signToolArguments.Append(CommandLineHelper.EncodeParameterArgument(password));
                removeLength = signToolArguments.Length - remove;
            }

            if (!string.IsNullOrEmpty(timestampUrl))
            {
                signToolArguments.Append(" /tr ");
                signToolArguments.Append(CommandLineHelper.EncodeParameterArgument(timestampUrl));

                // required in SDK builds 20236 and later
                // see https://docs.microsoft.com/en-us/dotnet/framework/tools/signtool-exe
                signToolArguments.Append(" /td SHA256");
            }

            foreach (var filePath in filePaths)
            {
                signToolArguments.Append(' ');
                signToolArguments.Append(CommandLineHelper.EncodeParameterArgument(filePath));
            }

            var args = signToolArguments.ToString();
            var maskedArgs = remove < 0 ? args : args.Remove(remove, removeLength).Insert(remove, "<removed-from-log>");

            var signTool = SdkPathHelper.GetSdkPath("signTool.exe", BundleHelper.SdkPath);
            Logger.Info().WriteLine("Executing {0} {1}", signTool, maskedArgs);
            
            try
            {
                await RunAsync(signTool, args, 0, cancellationToken).ConfigureAwait(false);
            }
            catch (ProcessWrapperException e)
            {
                var line = e.StandardError.FirstOrDefault(l => l.StartsWith("SignTool Error: "));
                if (line != null)
                {
                    if (TryGetErrorMessageFromSignToolOutput(e.StandardOutput, out var specialError))
                    {
                        throw new SdkException(string.Format(Resources.Localization.Infrastructure_Sdk_Error_SigningError_Format, "0x" + e.ExitCode.ToString("X2")) + " " + specialError, e.ExitCode);
                    }

                    throw new SdkException(string.Format(Resources.Localization.Infrastructure_Sdk_Error_SigningError_Format, "0x" + e.ExitCode.ToString("X2")) + " " + line.Substring("SignTool Error: ".Length), e.ExitCode);
                }

                if (e.ExitCode != 0)
                {
                    throw new SdkException(e.Message, e.ExitCode, e);
                }

                throw;
            }
        }

        public async Task SignWithPersonal(IEnumerable<string> filePaths, string algorithmType, string thumbprint, bool useMachineStore, string timestampUrl, CancellationToken cancellationToken = default)
        {
            var signToolArguments = new StringBuilder(256);
            
            signToolArguments.Append("sign");
            signToolArguments.Append(" /debug");
            signToolArguments.Append(" /fd ");
            signToolArguments.Append(CommandLineHelper.EncodeParameterArgument(algorithmType));

            if (useMachineStore)
            {
                signToolArguments.Append(" /sm");
            }

            if (!string.IsNullOrEmpty(timestampUrl))
            {
                signToolArguments.Append(" /tr ");
                signToolArguments.Append(CommandLineHelper.EncodeParameterArgument(timestampUrl));

                // required in SDK builds 20236 and later
                // see https://docs.microsoft.com/en-us/dotnet/framework/tools/signtool-exe
                signToolArguments.Append(" /td SHA256");
            }

            signToolArguments.Append(" /a /s MY ");
            signToolArguments.Append(" /sha1 ");
            signToolArguments.Append(CommandLineHelper.EncodeParameterArgument(thumbprint));

            foreach (var filePath in filePaths)
            {
                signToolArguments.Append(' ');
                signToolArguments.Append(CommandLineHelper.EncodeParameterArgument(filePath));
            }

            var args = signToolArguments.ToString();
            var signTool = SdkPathHelper.GetSdkPath("signTool.exe", BundleHelper.SdkPath);
            Logger.Info().WriteLine("Executing {0} {1}", signTool, args);

            try
            {
                await RunAsync(signTool, args, 0, default, cancellationToken).ConfigureAwait(false);
            }
            catch (ProcessWrapperException e)
            {
                string errorLine = null;

                var startIndex = e.StandardError.TakeWhile(l => !l.StartsWith("SignTool Error: ")).Count();
                if (startIndex < e.StandardError.Count)
                {
                    errorLine = e.StandardError[startIndex].Remove(0, "SignTool Error: ".Length).Trim();

                    var nextLines = e.StandardError.Skip(startIndex + 1).TakeWhile(l => !string.IsNullOrWhiteSpace(l) && !l.StartsWith("SignTool Error: "));
                    foreach (var nextLine in nextLines.Select(l => l.Trim()).Where(l => !string.IsNullOrWhiteSpace(l)))
                    {
                        errorLine += " " + nextLine;
                    }
                }

                if (errorLine != null)
                {
                    if (TryGetErrorMessageFromSignToolOutput(e.StandardOutput, out var specialError))
                    {
                        throw new SdkException(string.Format(Resources.Localization.Infrastructure_Sdk_Error_SigningError_Format, "0x" + e.ExitCode.ToString("X2")) + " " + specialError, e.ExitCode);
                    }

                    throw new SdkException(string.Format(Resources.Localization.Infrastructure_Sdk_Error_SigningError_Format, "0x" + e.ExitCode.ToString("X2")) + " " + errorLine, e.ExitCode);
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
                error = Resources.Localization.Infrastructure_Sdk_Error_SigningError_SubjectPublisherNotEqual;
                return true;
            }

            if (lines.Any(l => l.Contains("-2146869243/")))
            {
                error = string.Format(Resources.Localization.Infrastructure_Sdk_Error_SigningError_Timestamp_Format, "0x80096005");
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
                            error = Resources.Localization.Infrastructure_Sdk_Error_SigningError_NoPrivateKey;
                            break;
                        case "expiry":
                            error = Resources.Localization.Infrastructure_Sdk_Error_SigningError_Expired;
                            break;
                        case "eku":
                            error = Resources.Localization.Infrastructure_Sdk_Error_SigningError_EkuMismatch;
                            break;
                        default:
                            error = string.Format(Resources.Localization.Infrastructure_Sdk_Error_SigningError_Filter_Format, parsed[^1].Item2);
                            break;
                    }

                    return true;
                }
                else if (findHashIndex > 0)
                {
                    error = Resources.Localization.Infrastructure_Sdk_Error_SigningError_FailedValidation + " " + string.Join(", ", parsed.Skip(1).Take(findHashIndex - 1).Select(x => x.Item2));
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
                            error = Resources.Localization.Infrastructure_Sdk_Error_SigningError_NoPrivateKey;
                            break;
                        case "expiry":
                            error = Resources.Localization.Infrastructure_Sdk_Error_SigningError_Expired;
                            break;
                        case "eku":
                            error = Resources.Localization.Infrastructure_Sdk_Error_SigningError_EkuMismatch;
                            break;
                        default:
                            error = string.Format(Resources.Localization.Infrastructure_Sdk_Error_SigningError_Filter_Format, parsed[^1].Item2);
                            break;
                    }
                }
            }

            if (error != null)
            {
                Logger.Info().WriteLine(Resources.Localization.Infrastructure_Sdk_Error_SigningError_ExtraInfo + " " + string.Join(Environment.NewLine, lines.Where(l => !string.IsNullOrWhiteSpace(l))));
                return true;
            }

            return false;
        }
    }
}