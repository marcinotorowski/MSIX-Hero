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
using System.Threading;
using System.Threading.Tasks;
using Dapplo.Log;
using Otor.MsixHero.Appx.Editor.Commands.Concrete.Files;

namespace Otor.MsixHero.Appx.Editor.Executors.Concrete.Files
{
    public class AddFileExecutor : ExtractedAppxExecutor<AddFile>
    {
        private static readonly LogSource Logger = new();

        public AddFileExecutor(DirectoryInfo directory) : base(directory)
        {
        }

        public AddFileExecutor(string directory) : base(directory)
        {
        }

        public override Task Execute(AddFile command, CancellationToken cancellationToken = default)
        {
            var relativeTarget = this.ResolvePath(command.DestinationPath);
            var destination = new FileInfo(Path.Combine(this.Directory.FullName, relativeTarget));

            if (!File.Exists(command.SourcePath))
            {
                throw new FileNotFoundException($"File ('{command.SourcePath}') does not exist.");
            }

            if (destination.Directory?.Exists == false)
            {
                destination.Directory.Create();
            }
            
            Logger.Info().WriteLine($"Copying file from '{command.SourcePath}' to '{destination.FullName}'...");

            if (File.Exists(destination.FullName) && !command.Force)
            {
                throw new FileAlreadyExistsException(relativeTarget);
            }

            File.Copy(command.SourcePath, destination.FullName, command.Force);
            return Task.CompletedTask;
        }

        public class FileAlreadyExistsException : Exception
        {
            public FileAlreadyExistsException(string filePath)
            {
                FilePath = filePath;
            }

            public string FilePath { get; }
        }

    }
}
