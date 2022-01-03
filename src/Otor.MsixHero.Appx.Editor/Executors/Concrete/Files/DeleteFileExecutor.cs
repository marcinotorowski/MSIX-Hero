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
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Editor.Commands.Concrete.Files;
using Otor.MsixHero.Infrastructure.Logging;

namespace Otor.MsixHero.Appx.Editor.Executors.Concrete.Files
{
    public class DeleteFileExecutor : ExtractedAppxExecutor<DeleteFile>
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(DeleteFileExecutor));

        public DeleteFileExecutor(DirectoryInfo directory) : base(directory)
        {
        }

        public DeleteFileExecutor(string directory) : base(directory)
        {
        }

        public EventHandler<string> OnFileRemoved;

        public override Task Execute(DeleteFile command, CancellationToken cancellationToken = default)
        {
            var filePath = this.ResolvePath(command.FilePath);
            var lastSeparator = filePath.LastIndexOf('\\');
            var relativeDirectoryPath = lastSeparator < 0 ? null : filePath.Substring(0, lastSeparator);
            var fileName = lastSeparator < 0 ? filePath : Path.GetFileName(filePath);

            IList<FileInfo> filesToDelete;

            var cnt = 0;

            if (fileName.Contains('*') || fileName.Contains('?'))
            {
                if (relativeDirectoryPath == null)
                {
                    filesToDelete = this.Directory.EnumerateFiles(fileName, SearchOption.TopDirectoryOnly).ToList();
                }
                else
                {
                    var dir = new DirectoryInfo(this.Directory.FullName + "\\" + relativeDirectoryPath);
                    if (!dir.Exists)
                    {
                        Logger.Warn($"Part of the path ('{relativeDirectoryPath}') does not exist.");
                        filesToDelete = new List<FileInfo>();
                    }
                    else
                    {
                        // note: No Path.Combine due to wildcards!
                        dir = new DirectoryInfo(this.Directory.FullName + "\\" + relativeDirectoryPath);
                        filesToDelete = dir.EnumerateFiles(fileName, SearchOption.TopDirectoryOnly).ToList();
                    }
                }
            }
            else
            {
                filesToDelete = new List<FileInfo> { new FileInfo(Path.Combine(this.Directory.FullName, filePath)) };
            }

            if (filesToDelete.Any(f => string.Equals(f.Name, "AppxManifest.xml", StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException("Manifest file cannot be removed.");
            }

            foreach (var file in filesToDelete.Where(f => f != null))
            {
                if (!file.Exists)
                {
                    // ReSharper disable once AssignNullToNotNullAttribute
                    Logger.Warn($"File {Path.GetRelativePath(this.Directory.FullName, file.FullName)} does not exist.");
                }
                else
                {
                    try
                    {
                        file.Delete();
                        OnFileRemoved?.Invoke(this, Path.GetRelativePath(this.Directory.FullName, file.FullName));
                        cnt++;
                    }
                    catch (Exception e)
                    {
                        Logger.Warn($"File {Path.GetRelativePath(this.Directory.FullName, file.FullName)} could not be removed ({e.Message}).");
                    }
                }
            }

            if (cnt == 0)
            {
                Logger.Warn("No files have been removed.");
            }

            return Task.CompletedTask;
        }
    }
}
