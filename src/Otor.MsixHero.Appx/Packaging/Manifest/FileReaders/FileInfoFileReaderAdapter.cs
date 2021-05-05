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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;

namespace Otor.MsixHero.Appx.Packaging.Manifest.FileReaders
{
    public class FileInfoFileReaderAdapter : IAppxDiskFileReader
    {
        private readonly FileInfo appxManifestFile;

        public FileInfoFileReaderAdapter(FileInfo appxManifestFile)
        {
            if (!appxManifestFile.Exists)
            {
                throw new ArgumentException($"File {appxManifestFile.FullName} does not exist.", nameof(appxManifestFile));
            }

            this.RootDirectory = appxManifestFile.DirectoryName;
            this.appxManifestFile = appxManifestFile;
        }

        public FileInfoFileReaderAdapter(string appxManifestFile)
        {
            if (string.IsNullOrEmpty(appxManifestFile))
            {
                this.RootDirectory = null;
                this.appxManifestFile = null;
            }
            else
            {
                var fileInfo = new FileInfo(appxManifestFile);
                this.RootDirectory = fileInfo.DirectoryName;
                this.appxManifestFile = fileInfo;
            }
        }

        public string RootDirectory { get; }

        public Stream GetFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return null;
            }

            // ReSharper disable once PossibleNullReferenceException
            return File.OpenRead(Path.Combine(this.appxManifestFile.Directory.FullName, filePath));
        }

#pragma warning disable 1998
        public async IAsyncEnumerable<string> EnumerateDirectories(string rootRelativePath = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
#pragma warning restore 1998
        {
            // ReSharper disable once PossibleNullReferenceException
            var baseDir = this.appxManifestFile.Directory.FullName;
            var fullDir = rootRelativePath == null ? baseDir : Path.Combine(baseDir, rootRelativePath);

            foreach (var d in Directory.EnumerateDirectories(fullDir, "*", SearchOption.TopDirectoryOnly))
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return d;
            }
        }

#pragma warning disable 1998
        public async IAsyncEnumerable<AppxFileInfo> EnumerateFiles(string rootRelativePath, string wildcard, SearchOption searchOption = SearchOption.TopDirectoryOnly, [EnumeratorCancellation] CancellationToken cancellationToken = default)
#pragma warning restore 1998
        {
            // ReSharper disable once PossibleNullReferenceException
            var baseDir = this.appxManifestFile.Directory.FullName;
            var fullDir = rootRelativePath == null ? baseDir : Path.Combine(baseDir, rootRelativePath);

            foreach (var f in Directory.EnumerateFiles(fullDir, wildcard, searchOption))
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return new AppxFileInfo(new FileInfo(f));
            }
        }

        public IAsyncEnumerable<AppxFileInfo> EnumerateFiles(string rootRelativePath = null, CancellationToken cancellationToken = default)
        {
            return this.EnumerateFiles(rootRelativePath, "*", SearchOption.TopDirectoryOnly, cancellationToken);
        }

        public Stream GetResource(string resourceFilePath)
        {
            if (string.IsNullOrEmpty(resourceFilePath))
            {
                return null;
            }

            if (this.FileExists(resourceFilePath))
            {
                return this.GetFile(resourceFilePath);
            }

            var fileName = Path.GetFileName(resourceFilePath);
            var extension = Path.GetExtension(resourceFilePath);
            var resourceDir = Path.GetDirectoryName(resourceFilePath);

            var dirsToTry = new Queue<string>();
            dirsToTry.Enqueue(string.IsNullOrEmpty(resourceDir) ? this.appxManifestFile.DirectoryName : Path.Combine(this.appxManifestFile.DirectoryName, resourceDir));

            while (dirsToTry.Any())
            {
                var dequeued = dirsToTry.Dequeue();
                var dirInfo = new DirectoryInfo(dequeued);
                if (!dirInfo.Exists)
                {
                    continue;
                }

                var matchingFiles = dirInfo.EnumerateFiles(Path.GetFileNameWithoutExtension(fileName) + "*" + extension);
                foreach(var matchingFile in matchingFiles)
                {
                    var name = Regex.Replace(matchingFile.Name, @"\.[^\.\-]+-[^\.\-]+", string.Empty);
                    if (string.Equals(name, fileName, StringComparison.OrdinalIgnoreCase))
                    {
                        return this.GetFile(Path.GetRelativePath(this.appxManifestFile.DirectoryName, matchingFile.FullName));
                    }
                }

                var matchingDirectories = dirInfo.EnumerateDirectories().Where(d => Regex.IsMatch(dirInfo.Name, @".[^\.\-]+-[^\.\-]+"));
                foreach (var matchingDirectory in matchingDirectories)
                {
                    dirsToTry.Enqueue(matchingDirectory.FullName);
                }
            }

            return null;
        }

        public bool FileExists(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return false;
            }

            if (this.appxManifestFile?.Directory?.FullName == null)
            {
                return false;
            }

            // ReSharper disable once PossibleNullReferenceException
            return File.Exists(Path.Combine(this.appxManifestFile.Directory.FullName, filePath));
        }

        void IDisposable.Dispose()
        {
        }
    }
}