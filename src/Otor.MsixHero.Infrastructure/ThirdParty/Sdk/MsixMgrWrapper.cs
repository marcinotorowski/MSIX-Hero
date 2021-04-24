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
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Infrastructure.Logging;
using Otor.MsixHero.Infrastructure.ThirdParty.Exceptions;

namespace Otor.MsixHero.Infrastructure.ThirdParty.Sdk
{
    public class MsixMgrWrapper : ExeWrapper
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(MsixMgrWrapper));
        
        public enum FileType
        {
            Cim,
            Vhd,
            // ReSharper disable once IdentifierTypo
            Vhdx
        }
        
        public Task Unpack(
            string packageFilePath, 
            string containerPath, 
            FileType? fileType = null,
            uint size = 0,
            bool create = true,
            // ReSharper disable once IdentifierTypo
            bool applyAcls = true,
            string rootDirectory = null,
            CancellationToken cancellationToken = default)
        {
            var arguments = $"-Unpack -packagePath \"{packageFilePath}\" -destination \"{containerPath}\"";

            if (fileType.HasValue)
            {
                arguments += $" -fileType {fileType.Value.ToString("G").ToUpperInvariant()}";
            }

            if (size > 0)
            {
                arguments += $" -vhdSize \"{size}\"";
            }

            if (applyAcls)
            {
                // ReSharper disable once StringLiteralTypo
                arguments += " -applyacls";
            }
            
            if (create)
            {
                arguments += " -create";
            }

            if (!string.IsNullOrEmpty(rootDirectory))
            {
                arguments += $" -rootDirectory \"{rootDirectory}\"";
            }
            
            return this.RunMsixMgr(arguments, cancellationToken);
        }
        
        public static string GetMsixMgrPath(string localName, string baseDirectory = null)
        {
            var baseDir = baseDirectory ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "redistr", "msixmgr");
            var path = Path.Combine(baseDir, IntPtr.Size == 4 ? "x86" : "x64", localName);
            if (!File.Exists(path))
            {
                path = Path.Combine(baseDir, localName);
                if (!File.Exists(path))
                {
                    throw new FileNotFoundException($"Could not locale MSIXMGR part {path}.", path);
                }
            }

            return path;
        }

        private async Task RunMsixMgr(string arguments, CancellationToken cancellationToken, Action<string> callBack = null)
        {
            var msixmgr = GetMsixMgrPath("msixmgr.exe", BundleHelper.MsixMgrPath);
            Logger.Info("Executing {0} {1}", msixmgr, arguments);

            try
            {
                var tempDir = Path.GetTempPath();
                await RunAsync(msixmgr, arguments, tempDir, cancellationToken, callBack, 0).ConfigureAwait(false);
            }
            catch (ProcessWrapperException e)
            {
                if (e.ExitCode == -1951596541)
                {
                    throw new InvalidOperationException("Could not expand MSIX Package to the VHD file. The maximum size of the virtual disk is smaller than the file size of expanded MSIX package. Try using a bigger disk size.", e);
                }

                if (e.ExitCode == 0x80070522)
                {
                    throw new UnauthorizedAccessException("This operation requires admin permissions.", e);
                }

                throw new InvalidOperationException(e.StandardError.LastOrDefault(e => !string.IsNullOrWhiteSpace(e)), e);
            }
        }
    }
}
