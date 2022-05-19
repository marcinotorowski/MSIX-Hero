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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dapplo.Log;
using Otor.MsixHero.Infrastructure.Helpers;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.ThirdParty.Exceptions;

namespace Otor.MsixHero.Infrastructure.ThirdParty.Sdk
{
    public class DiskPartWrapper : ExeWrapper
    {
        private static readonly LogSource Logger = new();      
        
        public async Task MountVhd(string vhdPath, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            Logger.Info().WriteLine($"Mounting volume {vhdPath}...");
            var tempFile = Path.Combine(Path.GetTempPath(), "msix-hero-vhd-" + Guid.NewGuid().ToString("N").Substring(0, 10) + ".cfg");

            try
            {
                var content = @"select vdisk file = ""{0}""
attach vdisk";

                await File.WriteAllTextAsync(tempFile, string.Format(content, vhdPath), cancellationToken).ConfigureAwait(false);
                var arguments = new StringBuilder("/S ", 256);
                arguments.Append(CommandLineHelper.EncodeParameterArgument(tempFile));

                Logger.Debug().WriteLine($"DISKPART.EXE command in {tempFile}:\r\n{string.Format(content, vhdPath)}");
                await this.RunDiskPart(arguments.ToString(), cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    Logger.Debug().WriteLine($"Deleting {tempFile}...");
                    ExceptionGuard.Guard(() => File.Delete(tempFile));
                }
            }
        }

        public async Task CreateVhdAndAssignDriveLetter(string vhdPath, long requiredSize, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            Logger.Info().WriteLine($"Creating volume {vhdPath} with required size {requiredSize}");

            switch (Path.GetExtension(vhdPath).ToLowerInvariant())
            {
                case ".vhd":
                case ".vhdx":

                    var tempCreateFile = Path.Combine(Path.GetTempPath(), "msix-hero-vhd-" + Guid.NewGuid().ToString("N").Substring(0, 10) + ".cfg");

                    try
                    {
                        var content = @"create vdisk file=""{0}"" maximum={1} type=expandable";

                        var requiredSizeMb = (int)(10 * Math.Ceiling(0.1 * requiredSize / 1024 / 1024));
                        await File.WriteAllTextAsync(tempCreateFile, string.Format(content, vhdPath, requiredSizeMb), cancellationToken).ConfigureAwait(false);
                        
                        var arguments = new StringBuilder("/S ", 256);
                        arguments.Append(CommandLineHelper.EncodeParameterArgument(tempCreateFile));
                        
                        Logger.Debug().WriteLine($"DISKPART.EXE command in {tempCreateFile}:\r\n{string.Format(content, vhdPath, requiredSizeMb)}");
                        await this.RunDiskPart(arguments.ToString(), cancellationToken).ConfigureAwait(false);
                    }
                    finally
                    {
                        if (File.Exists(tempCreateFile))
                        {
                            Logger.Debug().WriteLine($"Deleting {tempCreateFile}...");
                            ExceptionGuard.Guard(() => File.Delete(tempCreateFile));
                        }
                    }

                    break;
                default:
                    throw new NotSupportedException($"Extension {Path.GetExtension(vhdPath)} is not supported.");
            }

            Logger.Info().WriteLine("Formatting drive and assigning drive letter...");
            string tempFileMount = null;
            try
            {
                var content = @"select vdisk file = ""{0}""
attach vdisk
create partition primary
format fs=ntfs
assign";
                tempFileMount = Path.Combine(Path.GetTempPath(), "msix-hero-vhd-" + Guid.NewGuid().ToString("N").Substring(0, 10) + ".cfg");
                await File.WriteAllTextAsync(tempFileMount, string.Format(content, vhdPath), cancellationToken).ConfigureAwait(false);
                
                var arguments = new StringBuilder("/S ", 256);
                arguments.Append(CommandLineHelper.EncodeParameterArgument(tempFileMount));
                
                Logger.Debug().WriteLine($"DISKPART.EXE command in {tempFileMount}:\r\n{string.Format(content, vhdPath)}");
                await this.RunDiskPart(arguments.ToString(), cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                if (File.Exists(tempFileMount))
                {
                    Logger.Debug().WriteLine($"Deleting {tempFileMount}...");
                    ExceptionGuard.Guard(() => File.Delete(tempFileMount));
                }
            }
        }

        public async Task DismountVhd(string vhdPath, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            Logger.Info().WriteLine("Dismounting volume...");
            var tempFile = Path.Combine(Path.GetTempPath(), "msix-hero-vhd-" + Guid.NewGuid().ToString("N").Substring(0, 10) + ".cfg");

            try
            {
                var content = @"select vdisk file = ""{0}""
detach vdisk";

                await File.WriteAllTextAsync(tempFile, string.Format(content, vhdPath), cancellationToken).ConfigureAwait(false);
                
                var arguments = new StringBuilder("/S ", 256);
                arguments.Append(CommandLineHelper.EncodeParameterArgument(tempFile));

                Logger.Debug().WriteLine($"DISKPART.EXE command in {tempFile}:\r\n{string.Format(content, vhdPath)}");
                await this.RunDiskPart(arguments.ToString(), cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    Logger.Debug().WriteLine($"Deleting {tempFile}...");
                    ExceptionGuard.Guard(() => File.Delete(tempFile));
                }
            }
        }

        private static string GetDiskPartPath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "diskpart.exe");
        }
        
        private async Task RunDiskPart(string arguments, CancellationToken cancellationToken, Action<string> callBack = null)
        {
            var diskPart = GetDiskPartPath();
            Logger.Info().WriteLine("Executing {0} {1}", diskPart, arguments);

            try
            {
                await RunAsync(diskPart, arguments, cancellationToken, callBack, 0).ConfigureAwait(false);
            }
            catch (ProcessWrapperException e)
            {
                throw new InvalidOperationException($"Diskpart.exe failed with error code {e.ExitCode:X2}", e);
            }
        }
    }
}
