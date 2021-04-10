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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Otor.MsixHero.Winget.Yaml
{
    [SuppressMessage("ReSharper", "IdentifierTypo")]
    [SuppressMessage("ReSharper", "CommentTypo")]
    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    public class WingetValidateWrapper
    {
        public async Task<string> GetWingetPath(CancellationToken cancellationToken)
        {
            var windowsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            var cmdPath = Path.Combine(windowsDirectory, "system32", "cmd.exe");
            const string cmdArgs = "/c where winget";

            var winGetLocator = new ProcessStartInfo(cmdPath, cmdArgs)
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            var proc = new Process
            {
                StartInfo = winGetLocator
            };

            proc.Start();

            var output = await proc.StandardOutput.ReadToEndAsync().ConfigureAwait(false);
            await proc.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
            var exitCode = proc.ExitCode;
            if (exitCode == 0 && !string.IsNullOrWhiteSpace(output))
            {
                try
                {
                    var fileInfo = new FileInfo(output.Trim());
                    if (fileInfo.Exists)
                    {
                        return fileInfo.FullName;
                    }
                }
                catch (ArgumentException)
                {
                }
            }

            return null;
        }

        /// <summary>
        /// Validates with help of winget.exe if the manifest is OK.
        /// </summary>
        /// <param name="yamlPath">The path to YAML file.</param>
        /// <param name="throwIfWinGetMissing">Whether to throw exceptions if winget is missing / not installed. When set to false, lack of winget means the validation is still successful.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Error message if validation failed, null if it succeeded.</returns>
        public async Task<string> ValidateAsync(string yamlPath, bool throwIfWinGetMissing = false, CancellationToken cancellationToken = default)
        {
            var validationDetails = await this.GetRawValidationAsync(yamlPath, throwIfWinGetMissing, cancellationToken).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(validationDetails))
            {
                if (validationDetails.Contains("Manifest Warning: Unknown field. Field: ManifestType") || validationDetails.Contains("Unsupported ManifestVersion: 1.0.0"))
                {
                    // This indicates that the winget that is installed is in too low version, so we return no errors.
                    return "Your Winget version is too old to perform the validation against the manifest schema v1.";
                }
                
                return validationDetails.IndexOf("Manifest validation succeeded.", StringComparison.OrdinalIgnoreCase) == -1 ? string.Join(Environment.NewLine, validationDetails.Split(Environment.NewLine).Skip(1)) : null;
            }

            return null;
        }

        private async Task<string> GetRawValidationAsync(string yamlPath, bool throwIfWinGetMissing, CancellationToken cancellationToken = default)
        {
            var winGetPath = await this.GetWingetPath(cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrEmpty(winGetPath))
            {
                if (throwIfWinGetMissing)
                {
                    throw new InvalidOperationException("Could not start winget.");
                }

                return null;
            }
            
            var processStartInfo = new ProcessStartInfo(winGetPath, $"validate \"{yamlPath}\"")
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            var winGetProcess = new Process()
            {
                StartInfo = processStartInfo
            };

            winGetProcess.Start();

            var standardOutput = await winGetProcess.StandardOutput.ReadToEndAsync().ConfigureAwait(false);
            await winGetProcess.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

            return standardOutput;
        }
    }
}