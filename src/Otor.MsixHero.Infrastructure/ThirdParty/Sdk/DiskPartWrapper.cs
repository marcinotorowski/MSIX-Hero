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
            Logger.Info().WriteLine(Resources.Localization.Infrastructure_Sdk_VhdMount_Format, vhdPath);
            var tempFile = Path.Combine(Path.GetTempPath(), "msix-hero-vhd-" + Guid.NewGuid().ToString("N").Substring(0, 10) + ".cfg");

            try
            {
                var content = @"select vdisk file = ""{0}""
attach vdisk";

                await File.WriteAllTextAsync(tempFile, string.Format(content, vhdPath), cancellationToken).ConfigureAwait(false);
                var arguments = new StringBuilder("/S ", 256);
                arguments.Append(CommandLineHelper.EncodeParameterArgument(tempFile));

                Logger.Debug().WriteLine(string.Format(Resources.Localization.Infrastructure_Sdk_DiskPartCommand_Format, tempFile) + $"\r\n{string.Format(content, vhdPath)}");
                await this.RunDiskPart(arguments.ToString(), cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    Logger.Debug().WriteLine(Resources.Localization.Infrastructure_Sdk_DeletingFile_Format, tempFile);
                    ExceptionGuard.Guard(() => File.Delete(tempFile));
                }
            }
        }

        public async Task CreateVhdAndAssignDriveLetter(string vhdPath, long requiredSize, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            Logger.Info().WriteLine(Resources.Localization.Infrastructure_Sdk_CreatingVolume_Format, vhdPath, requiredSize);

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
                        var arguments = $"/S \"{tempCreateFile}\"";

                        Logger.Debug().WriteLine(string.Format(Resources.Localization.Infrastructure_Sdk_DiskPartCommand_Format, tempCreateFile) + $"\r\n{string.Format(content, vhdPath)}");
                        await this.RunDiskPart(arguments.ToString(), cancellationToken).ConfigureAwait(false);
                    }
                    finally
                    {
                        if (File.Exists(tempCreateFile))
                        {
                            Logger.Debug().WriteLine(Resources.Localization.Infrastructure_Sdk_DeletingFile_Format, tempCreateFile);
                            ExceptionGuard.Guard(() => File.Delete(tempCreateFile));
                        }
                    }

                    break;
                default:
                    throw new NotSupportedException(string.Format(Resources.Localization.Infrastructure_Sdk_Error_NotSupportedExtension_Format, Path.GetExtension(vhdPath)));
            }

            Logger.Info().WriteLine(Resources.Localization.Infrastructure_Sdk_FormattingAndAssigning);
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
                
                Logger.Debug().WriteLine(string.Format(Resources.Localization.Infrastructure_Sdk_DiskPartCommand_Format, tempFileMount) + $"\r\n{string.Format(content, vhdPath)}");
                await this.RunDiskPart(arguments.ToString(), cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                if (File.Exists(tempFileMount))
                {
                    Logger.Debug().WriteLine(Resources.Localization.Infrastructure_Sdk_DeletingFile_Format, tempFileMount);
                    ExceptionGuard.Guard(() => File.Delete(tempFileMount));
                }
            }
        }

        public async Task DismountVhd(string vhdPath, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            Logger.Info().WriteLine(Resources.Localization.Infrastructure_Sdk_VhdDismount);
            var tempFile = Path.Combine(Path.GetTempPath(), "msix-hero-vhd-" + Guid.NewGuid().ToString("N").Substring(0, 10) + ".cfg");

            try
            {
                var content = @"select vdisk file = ""{0}""
detach vdisk";

                await File.WriteAllTextAsync(tempFile, string.Format(content, vhdPath), cancellationToken).ConfigureAwait(false);
                
                var arguments = new StringBuilder("/S ", 256);
                arguments.Append(CommandLineHelper.EncodeParameterArgument(tempFile));

                Logger.Debug().WriteLine(string.Format(Resources.Localization.Infrastructure_Sdk_DiskPartCommand_Format, tempFile) + $"\r\n{string.Format(content, vhdPath)}");
                await this.RunDiskPart(arguments.ToString(), cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    Logger.Debug().WriteLine(Resources.Localization.Infrastructure_Sdk_DeletingFile_Format, tempFile);
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
                throw new InvalidOperationException(string.Format(Resources.Localization.Infrastructure_Sdk_Error_DiskPart_Format, e.ExitCode.ToString("X2")), e);
            }
        }
    }
}
