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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dapplo.Log;
using Otor.MsixHero.Infrastructure.Helpers;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.ThirdParty.Exceptions;

namespace Otor.MsixHero.Infrastructure.ThirdParty.Sdk
{
    [SuppressMessage("ReSharper", "IdentifierTypo")]
    public class MsixMgrWrapper : ExeWrapper
    {
        private static readonly LogSource Logger = new();

        private readonly bool _forceRunFromSource;

        /// <summary>
        /// Initializes a new instance of <see cref="MsixMgrWrapper"/> class.
        /// </summary>
        /// <param name="forceRunFromSource">If <c>true</c>, then the original msixmgr.exe is always started. If this is false, then
        /// MSIXMGR may be extracted and started from a temporary location if running from MSIX. This is to overcome a serious bug
        /// in its implementation, where the temporary files are created not in temp folders, but rather in its working directory,
        /// which if running from MSIX is read-only.</param>
        public MsixMgrWrapper(bool forceRunFromSource = false)
        {
            this._forceRunFromSource = forceRunFromSource;
        }

        public enum FileType
        {
            Cim,
            Vhd,
            // ReSharper disable once IdentifierTypo
            Vhdx
        }

        public Task Unpack(string packageFilePath, string unpackedDirectory, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            var arguments = new StringBuilder("-Unpack ", 256);

            arguments.Append(" -packagePath");
            arguments.Append(CommandLineHelper.EncodeParameterArgument(packageFilePath));
            
            arguments.Append(" -destination");
            arguments.Append(CommandLineHelper.EncodeParameterArgument(unpackedDirectory));
            
            return this.RunMsixMgr(arguments.ToString(), cancellationToken);
        }

        public Task ApplyAcls(string unpackedPackageDirectory, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            var arguments = new StringBuilder("-ApplyACLs ", 256);

            arguments.Append(" -packagePath");
            arguments.Append(CommandLineHelper.EncodeParameterArgument(unpackedPackageDirectory));

            return this.RunMsixMgr(arguments.ToString(), cancellationToken);
        }

        public Task UnpackEx(
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
            var arguments = new StringBuilder("-Unpack ", 256);

            arguments.Append(" -packagePath");
            arguments.Append(CommandLineHelper.EncodeParameterArgument(packageFilePath));

            arguments.Append(" -destination");
            arguments.Append(CommandLineHelper.EncodeParameterArgument(containerPath));
            
            if (fileType.HasValue)
            {
                arguments.Append(" -fileType");
                arguments.Append(CommandLineHelper.EncodeParameterArgument(fileType.Value.ToString("G").ToUpperInvariant()));
            }

            if (size > 0)
            {
                arguments.Append(" -vhdSize");
                arguments.Append(size);
            }

            if (applyAcls)
            {
                // ReSharper disable once StringLiteralTypo
                arguments.Append(" -applyacls");
            }
            
            if (create)
            {
                arguments.Append(" -create");
            }

            if (!string.IsNullOrEmpty(rootDirectory))
            {
                arguments.Append(" -rootDirectory");
                arguments.Append(CommandLineHelper.EncodeParameterArgument(rootDirectory));
            }
            
            return this.RunMsixMgr(arguments.ToString(), cancellationToken);
        }
        
        public static string GetMsixMgrPath(string localName, string baseDirectory = null)
        {
            // ReSharper disable once StringLiteralTypo
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

        private async Task RunMsixMgr(string arguments, CancellationToken cancellationToken = default, Action<string> callBack = null)
        {
            string msixMgrPath;
            string msixMgrDirectory;
            bool cleanupMsixMgrDirectory;

            if (!this._forceRunFromSource && new DesktopBridge.Helpers().IsRunningAsUwp())
            {
                Logger.Info().WriteLine(Resources.Localization.Infrastructure_Sdk_MsixMgr_RunningWithIdentity);
                msixMgrDirectory = Path.Combine(Path.GetTempPath(), "msixmgr-" + Guid.NewGuid().ToString("N").Substring(0, 10));
                
                var originalMsixMgrPath = GetMsixMgrPath("msixmgr.exe", BundleHelper.MsixMgrPath);
                var originalMsixMgrDirectory = Path.GetDirectoryName(originalMsixMgrPath);
                if (originalMsixMgrDirectory == null)
                {
                    throw new InvalidOperationException(Resources.Localization.Infrastructure_Sdk_MsixMgr_Error_OriginalDirUnavailable);
                }

                foreach (var item in Directory.EnumerateFiles(originalMsixMgrDirectory, "*.*", SearchOption.AllDirectories))
                {
                    var relativePath = Path.GetRelativePath(originalMsixMgrDirectory, item);
                    var sourceFile = new FileInfo(Path.Combine(originalMsixMgrDirectory, relativePath));
                    var targetFile = new FileInfo(Path.Combine(msixMgrDirectory, relativePath));

                    if (targetFile.Directory?.Exists == false)
                    {
                        targetFile.Directory.Create();
                    }

                    Logger.Debug().WriteLine(Resources.Localization.Infrastructure_Sdk_MsixMgr_CopyingFile, sourceFile.FullName, targetFile.FullName);
                    sourceFile.CopyTo(targetFile.FullName);
                }

                msixMgrPath = Path.Combine(msixMgrDirectory, "msixmgr.exe");
                cleanupMsixMgrDirectory = true;
            }
            else
            {
                msixMgrPath = GetMsixMgrPath("msixmgr.exe", BundleHelper.MsixMgrPath);
                msixMgrDirectory = Path.GetDirectoryName(msixMgrPath);
                cleanupMsixMgrDirectory = false;
            }
            
            Logger.Info().WriteLine(Resources.Localization.Infrastructure_Sdk_Executing_Format, msixMgrPath, arguments);

            try
            {
                var tempDir = Path.GetTempPath();
                await RunAsync(msixMgrPath, arguments, tempDir, cancellationToken, callBack, 0).ConfigureAwait(false);
            }
            catch (ProcessWrapperException e)
            {
                Logger.Warn().WriteLine(string.Format(Resources.Localization.Infrastructure_Sdk_MsixMgr_ProcessExitCode, e.ExitCode));
                if (e.ExitCode == -1951596541)
                {
                    throw new InvalidOperationException(Resources.Localization.Infrastructure_Sdk_MsixMgr_Error_TooSmallSize, e);
                }

#pragma warning disable 652
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                if (e.ExitCode == 0x80070522)
#pragma warning restore 652
                {
                    throw new UnauthorizedAccessException(Resources.Localization.Infrastructure_Sdk_MsixMgr_Error_AdminRights, e);
                }

                throw new InvalidOperationException(e.StandardError.LastOrDefault(stdError => !string.IsNullOrWhiteSpace(stdError) && !stdError.Contains("Successfully started the Shell Hardware Detection Service", StringComparison.OrdinalIgnoreCase)), e);
            }
            finally
            {
                if (cleanupMsixMgrDirectory)
                {
                    Logger.Debug().WriteLine(Resources.Localization.Infrastructure_Sdk_MsixMgr_RemovingTempFolder_Format, msixMgrDirectory);
                    ExceptionGuard.Guard(() => Directory.Delete(msixMgrDirectory, true));
                }
            }
        }
    }
}
