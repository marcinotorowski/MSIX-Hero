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
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Infrastructure.Logging;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.ThirdParty.Exceptions;

namespace Otor.MsixHero.Infrastructure.ThirdParty.Sdk
{
    public class MsixMgrWrapper : ExeWrapper
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(MsixMgrWrapper));
        
        public Task Unpack(string packageFilePath, string unpackedDirectory, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            var arguments = $"-Unpack -packagePath \"{packageFilePath}\" -destination \"{unpackedDirectory}\"";
            return this.RunMsixMgr(arguments, cancellationToken);
        }

        public Task ApplyAcls(string unpackedPackageDirectory, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            var arguments = $"-ApplyACLs -packagePath \"{unpackedPackageDirectory}\"";
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
                await RunAsync(msixmgr, arguments, cancellationToken, callBack, 0).ConfigureAwait(false);
            }
            catch (ProcessWrapperException e)
            {
                if (e.ExitCode == -1951596541)
                {
                    throw new InvalidOperationException("Could not expand MSIX Package to the VHD file. The maximum size of the virtual disk is smaller than the file size of expanded MSIX package. Try using a bigger disk size.", e);
                }

                throw new InvalidOperationException($"Expanding of MSIX package to a VHD image failed with error code {e.ExitCode:X2}", e);
            }
        }
    }
}
