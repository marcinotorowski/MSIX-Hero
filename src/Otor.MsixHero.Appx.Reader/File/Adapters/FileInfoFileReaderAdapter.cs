// MSIX Hero
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
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using Otor.MsixHero.Appx.Reader.File;
using Otor.MsixHero.Appx.Reader.File.Entities;

namespace Otor.MsixHero.Appx.Reader.File.Adapters
{
    public class FileInfoFileReaderAdapter : IAppxDiskFileReader
    {
        private readonly FileInfo _appxManifestFile;

        public FileInfoFileReaderAdapter(FileInfo appxManifestFile)
        {
            if (!appxManifestFile.Exists)
            {
                throw new ArgumentException(string.Format(Resources.Localization.Packages_Error_FileNotFound_Format, appxManifestFile.FullName), nameof(appxManifestFile));
            }

            RootDirectory = appxManifestFile.DirectoryName;
            _appxManifestFile = appxManifestFile;
            FilePath = appxManifestFile.FullName;
        }

        public FileInfoFileReaderAdapter(string appxManifestFile)
        {
            FilePath = appxManifestFile;
            if (string.IsNullOrEmpty(appxManifestFile))
            {
                RootDirectory = null;
                _appxManifestFile = null;
            }
            else
            {
                var fileInfo = new FileInfo(appxManifestFile);
                RootDirectory = fileInfo.DirectoryName;
                _appxManifestFile = fileInfo;
            }
        }

        public string RootDirectory { get; }

        public string FilePath { get; }

        public Stream GetFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return null;
            }

            // ReSharper disable once PossibleNullReferenceException
            return System.IO.File.OpenRead(Path.Combine(_appxManifestFile.Directory.FullName, filePath));
        }

#pragma warning disable 1998
        public async IAsyncEnumerable<string> EnumerateDirectories(string rootRelativePath = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
#pragma warning restore 1998
        {
            // ReSharper disable once PossibleNullReferenceException
            var baseDir = _appxManifestFile.Directory.FullName;
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
            var baseDir = _appxManifestFile.Directory.FullName;
            var fullDir = rootRelativePath == null ? baseDir : Path.Combine(baseDir, rootRelativePath);

            foreach (var f in Directory.EnumerateFiles(fullDir, wildcard, searchOption))
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return new AppxFileInfo(new FileInfo(f));
            }
        }

        public IAsyncEnumerable<AppxFileInfo> EnumerateFiles(string rootRelativePath = null, CancellationToken cancellationToken = default)
        {
            return EnumerateFiles(rootRelativePath, "*", SearchOption.TopDirectoryOnly, cancellationToken);
        }

        public Stream GetResource(string resourceFilePath)
        {
            if (string.IsNullOrEmpty(resourceFilePath))
            {
                return null;
            }

            if (FileExists(resourceFilePath))
            {
                return GetFile(resourceFilePath);
            }

            var fileName = Path.GetFileName(resourceFilePath);
            var extension = Path.GetExtension(resourceFilePath);
            var resourceDir = Path.GetDirectoryName(resourceFilePath);

            var dirsToTry = new Queue<string>();
            // ReSharper disable once AssignNullToNotNullAttribute
            dirsToTry.Enqueue(string.IsNullOrEmpty(resourceDir) ? _appxManifestFile.DirectoryName : Path.Combine(_appxManifestFile.DirectoryName, resourceDir));

            while (dirsToTry.Any())
            {
                var dequeued = dirsToTry.Dequeue();
                var dirInfo = new DirectoryInfo(dequeued);
                if (!dirInfo.Exists)
                {
                    continue;
                }

                var matchingFiles = dirInfo.EnumerateFiles(Path.GetFileNameWithoutExtension(fileName) + "*" + extension);
                foreach (var matchingFile in matchingFiles)
                {
                    var name = Regex.Replace(matchingFile.Name, @"\.[^\.\-]+-[^\.\-]+", string.Empty);
                    if (string.Equals(name, fileName, StringComparison.OrdinalIgnoreCase))
                    {
                        // ReSharper disable once AssignNullToNotNullAttribute
                        return GetFile(Path.GetRelativePath(_appxManifestFile.DirectoryName, matchingFile.FullName));
                    }
                }

                var matchingDirectories = dirInfo.EnumerateDirectories().Where(d => Regex.IsMatch(d.Name, @".[^\.\-]+-[^\.\-]+"));
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

            if (_appxManifestFile?.Directory?.FullName == null)
            {
                return false;
            }

            // ReSharper disable once PossibleNullReferenceException
            return System.IO.File.Exists(Path.Combine(_appxManifestFile.Directory.FullName, filePath));
        }

        public bool DirectoryExists(string directoryPath)
        {
            if (string.IsNullOrEmpty(directoryPath))
            {
                return true;
            }

            if (_appxManifestFile?.Directory?.FullName == null)
            {
                return false;
            }

            // ReSharper disable once PossibleNullReferenceException
            return Directory.Exists(Path.Combine(_appxManifestFile.Directory.FullName, directoryPath));
        }

        void IDisposable.Dispose()
        {
        }
    }
}