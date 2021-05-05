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
using System.Threading;

namespace Otor.MsixHero.Appx.Packaging.Manifest.FileReaders
{
    public class DirectoryInfoFileReaderAdapter : IAppxDiskFileReader
    {
        private readonly IAppxFileReader adapter;

        public DirectoryInfoFileReaderAdapter(DirectoryInfo appxManifestFolder)
        {
            if (!appxManifestFolder.Exists)
            {
                throw new ArgumentException($"Directory {appxManifestFolder.FullName} does not exist.");
            }

            this.RootDirectory = appxManifestFolder.FullName;

            var appxManifest = Path.Combine(appxManifestFolder.FullName, FileConstants.AppxManifestFile);
            if (!File.Exists(appxManifest))
            {
                appxManifest = Path.Combine(appxManifestFolder.FullName, FileConstants.AppxBundleManifestFilePath);
                if (!File.Exists(appxManifest))
                {
                    throw new ArgumentException("This folder does not contain APPX/MSIX package nor APPX bundle.", nameof(appxManifestFolder));
                }

                adapter = new FileInfoFileReaderAdapter(appxManifest);
            }
            else
            {
                adapter = new FileInfoFileReaderAdapter(Path.Combine(appxManifestFolder.FullName, FileConstants.AppxManifestFile));
            }
        }

        public DirectoryInfoFileReaderAdapter(string appxManifestFolder) : this(new DirectoryInfo(appxManifestFolder))
        {
        }

        public string RootDirectory { get; }

        public Stream GetFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return null;
            }

            return this.adapter.GetFile(filePath);
        }

        public IAsyncEnumerable<string> EnumerateDirectories(string rootRelativePath = null, CancellationToken cancellationToken = default)
        {
            return this.adapter.EnumerateDirectories(rootRelativePath, cancellationToken);
        }

        public IAsyncEnumerable<AppxFileInfo> EnumerateFiles(string rootRelativePath, string wildcard, SearchOption searchOption = SearchOption.TopDirectoryOnly, CancellationToken cancellationToken = default)
        {
            return this.adapter.EnumerateFiles(rootRelativePath, wildcard, searchOption, cancellationToken);
        }

        public IAsyncEnumerable<AppxFileInfo> EnumerateFiles(string rootRelativePath = null, CancellationToken cancellationToken = default)
        {
            return this.adapter.EnumerateFiles(rootRelativePath, cancellationToken);
        }

        public bool FileExists(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return false;
            }

            return this.adapter.FileExists(filePath);
        }

        public Stream GetResource(string resourceFilePath)
        {
            if (string.IsNullOrEmpty(resourceFilePath))
            {
                return null;
            }

            return this.adapter.GetResource(resourceFilePath);
        }

        void IDisposable.Dispose()
        {
            this.adapter.Dispose();
        }
    }
}