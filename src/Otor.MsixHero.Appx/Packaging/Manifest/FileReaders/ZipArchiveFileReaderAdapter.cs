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
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using Otor.MsixHero.Infrastructure.Helpers;

namespace Otor.MsixHero.Appx.Packaging.Manifest.FileReaders
{
    public class ZipArchiveFileReaderAdapter : IAppxFileReader
    {
        private readonly string msixPackagePath;
        private ZipArchive msixPackage;
        private IDisposable[] disposableStreams;

        public ZipArchiveFileReaderAdapter(ZipArchive msixPackage, string msixPackagePath = null)
        {
            this.PackagePath = msixPackagePath;
            this.msixPackage = msixPackage;
        }

        public ZipArchiveFileReaderAdapter(string msixPackagePath)
        {
            this.PackagePath = msixPackagePath;
            this.msixPackagePath = msixPackagePath;
        }

        public ZipArchiveFileReaderAdapter(FileStream msixPackage, bool ownStream = true)
        {
            this.msixPackagePath = msixPackage.Name;
            this.msixPackage = new ZipArchive(msixPackage, ZipArchiveMode.Read, !ownStream);

            if (ownStream)
            {
                this.disposableStreams = new IDisposable[] { msixPackage, this.msixPackage };
            }
            else
            {
                this.disposableStreams = new IDisposable[] { this.msixPackage };
            }
        }

        public string PackagePath { get; private set; }

#pragma warning disable 1998
        public async IAsyncEnumerable<string> EnumerateDirectories(string rootRelativePath = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
#pragma warning restore 1998
        {
            if (string.IsNullOrEmpty(rootRelativePath))
            {
                rootRelativePath = string.Empty;
            }
            else
            {
                rootRelativePath = rootRelativePath.Replace(Path.DirectorySeparatorChar, '/').TrimEnd('/') + '/';
            }

            var hashset = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            
            this.EnsureInitialized();
            foreach (var entry in msixPackage.Entries)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!entry.FullName.StartsWith(rootRelativePath, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                
                var firstSlash = entry.FullName.IndexOf('/', rootRelativePath.Length);
                if (firstSlash == -1)
                {
                    continue;
                }

                var candidate = entry.FullName.Substring(0, firstSlash);
                if (hashset.Add(candidate))
                {
                    yield return candidate.Replace('/', Path.DirectorySeparatorChar);
                }
            }
        }

#pragma warning disable 1998
        public async IAsyncEnumerable<AppxFileInfo> EnumerateFiles(string rootRelativePath, string wildcard, SearchOption searchOption = SearchOption.TopDirectoryOnly, [EnumeratorCancellation] CancellationToken cancellationToken = default)
#pragma warning restore 1998
        {
            if (string.IsNullOrEmpty(rootRelativePath))
            {
                rootRelativePath = string.Empty;
            }
            else
            {
                rootRelativePath = rootRelativePath.Replace(Path.DirectorySeparatorChar, '/').TrimEnd('/') + '/';
            }

            var regex = string.IsNullOrEmpty(wildcard) ? null : RegexBuilder.FromWildcard(wildcard);

            this.EnsureInitialized();
            foreach (var entry in msixPackage.Entries)
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                if (!entry.FullName.StartsWith(rootRelativePath, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string fileName;
                var lastSlash = entry.FullName.Substring(rootRelativePath.Length).LastIndexOf('/');
                if (lastSlash != -1)
                {
                    fileName = entry.FullName.Substring(lastSlash + 1);
                }
                else
                {
                    fileName = entry.FullName.Substring(rootRelativePath.Length);
                }
                
                if (searchOption == SearchOption.AllDirectories)
                {
                    if (regex == null || regex.IsMatch(fileName))
                    {
                        yield return new AppxFileInfo(entry.FullName.Replace('/', Path.DirectorySeparatorChar), entry.Length);
                    }
                }
                else
                {
                    if (entry.FullName.IndexOf('/', rootRelativePath.Length) == -1)
                    {
                        if (regex == null || regex.IsMatch(fileName))
                        {
                            yield return new AppxFileInfo(entry.FullName.Replace('/', Path.DirectorySeparatorChar), entry.Length);
                        }
                    }
                }
            }
        }

        public IAsyncEnumerable<AppxFileInfo> EnumerateFiles(string rootRelativePath = null, CancellationToken cancellationToken = default)
        {
            return this.EnumerateFiles(rootRelativePath, "*", SearchOption.TopDirectoryOnly, cancellationToken);
        }

        public Stream GetFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            this.EnsureInitialized();

            var entry = msixPackage.GetEntry(filePath.Replace(Path.DirectorySeparatorChar, '/'));
            if (entry == null)
            {
                entry = msixPackage.Entries.FirstOrDefault(e => string.Equals(e.FullName, filePath.Replace(Path.DirectorySeparatorChar, '/'), StringComparison.OrdinalIgnoreCase));
                if (entry == null)
                {
                    throw new FileNotFoundException($"File {filePath} not found in MSIX package.");
                }
            }

            return entry.Open();
        }

        public Stream GetResource(string resourceFilePath)
        {
            if (string.IsNullOrEmpty(resourceFilePath))
            {
                return null;
            }

            this.EnsureInitialized();

            if (this.FileExists(resourceFilePath))
            {
                return this.GetFile(resourceFilePath);
            }

            var resourceDir = Path.GetDirectoryName(resourceFilePath) + "/";
            var resourceFileName = Path.GetFileName(resourceFilePath);

            foreach (var item in this.msixPackage.Entries)
            {
                var currentName = item.FullName;
                if (resourceDir.Length > 1)
                {
                    if (!currentName.StartsWith(resourceDir, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    currentName = currentName.Remove(0, resourceDir.Length);
                }

                // 1) Remove quantified folder names
                currentName = Regex.Replace(currentName, @"[^\.\-]+-[^\.\-]+[\\/]", string.Empty);
                
                // 2) Remove quantified file names
                currentName = Regex.Replace(currentName, @"\.[^\.\-]+-[^\.\-]+", string.Empty);

                if (string.Equals(currentName, resourceFileName, StringComparison.OrdinalIgnoreCase))
                {
                    return item.Open();
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

            this.EnsureInitialized();

            filePath = filePath.Replace(Path.DirectorySeparatorChar, '/');
            var entry = this.msixPackage.GetEntry(filePath);
            if (entry != null)
            {
                return true;
            }

            return this.msixPackage.Entries.Any(e => string.Equals(filePath, e.FullName, StringComparison.OrdinalIgnoreCase));
        }

        public void Dispose()
        {
            if (this.disposableStreams == null)
            {
                return;
            }

            foreach (var item in this.disposableStreams)
            {
                item.Dispose();
            }
        }
        private void EnsureInitialized()
        {
            if (this.msixPackage != null)
            {
                return;
            }

            if (!File.Exists(this.msixPackagePath))
            {
                throw new ArgumentException($"File { this.msixPackagePath} does not exist.");
            }

            var fileStream = File.OpenRead(msixPackagePath);

            try
            {
                this.msixPackage = new ZipArchive(fileStream, ZipArchiveMode.Read, false);
                this.disposableStreams = new IDisposable[] { this.msixPackage, fileStream };
            }
            catch (InvalidDataException e)
            {
                this.disposableStreams = new IDisposable[] { fileStream };
                throw new InvalidDataException("This file is not an MSIX/APPX package, or the content of the package is damaged.", e);
            }
            catch (Exception)
            {
                this.disposableStreams = new IDisposable[] { fileStream };
                throw;
            }
        }
    }
}