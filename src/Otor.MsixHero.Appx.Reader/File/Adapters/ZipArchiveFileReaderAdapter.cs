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
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using Otor.MsixHero.Appx.Reader.File.Entities;
using Otor.MsixHero.Infrastructure.Helpers;

namespace Otor.MsixHero.Appx.Reader.File.Adapters
{
    public class ZipArchiveFileReaderAdapter : IAppxFileReader
    {
        private readonly string _msixPackagePath;
        private ZipArchive _msixPackage;
        private IDisposable[] _disposableStreams;

        public ZipArchiveFileReaderAdapter(ZipArchive msixPackage, string msixPackagePath = null)
        {
            PackagePath = msixPackagePath;
            _msixPackage = msixPackage;
        }

        public ZipArchiveFileReaderAdapter(string msixPackagePath)
        {
            PackagePath = msixPackagePath;
            _msixPackagePath = msixPackagePath;
        }

        public ZipArchiveFileReaderAdapter(FileStream msixPackage, bool ownStream = true)
        {
            _msixPackagePath = msixPackage.Name;
            _msixPackage = new ZipArchive(msixPackage, ZipArchiveMode.Read, !ownStream);

            if (ownStream)
            {
                _disposableStreams = new IDisposable[] { msixPackage, _msixPackage };
            }
            else
            {
                _disposableStreams = new IDisposable[] { _msixPackage };
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

            EnsureInitialized();
            foreach (var entry in _msixPackage.Entries)
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

            EnsureInitialized();
            foreach (var entry in _msixPackage.Entries)
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
            return EnumerateFiles(rootRelativePath, "*", SearchOption.TopDirectoryOnly, cancellationToken);
        }

        public Stream GetFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            EnsureInitialized();

            var entry = _msixPackage.GetEntry(filePath.Replace(Path.DirectorySeparatorChar, '/'));
            if (entry == null)
            {
                entry = _msixPackage.Entries.FirstOrDefault(e => string.Equals(e.FullName, filePath.Replace(Path.DirectorySeparatorChar, '/'), StringComparison.OrdinalIgnoreCase));
                if (entry == null)
                {
                    throw new FileNotFoundException(string.Format(Resources.Localization.Packages_Error_FileInsideNotFound, filePath));
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

            EnsureInitialized();

            if (FileExists(resourceFilePath))
            {
                return GetFile(resourceFilePath);
            }

            var resourceDir = Path.GetDirectoryName(resourceFilePath) + "/";
            var resourceFileName = Path.GetFileName(resourceFilePath);

            foreach (var item in _msixPackage.Entries)
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

            EnsureInitialized();

            filePath = filePath.Replace(Path.DirectorySeparatorChar, '/');
            var entry = _msixPackage.GetEntry(filePath);
            if (entry != null)
            {
                return true;
            }

            return _msixPackage.Entries.Any(e => string.Equals(filePath, e.FullName, StringComparison.OrdinalIgnoreCase));
        }

        public bool DirectoryExists(string directoryPath)
        {
            if (string.IsNullOrEmpty(directoryPath))
            {
                return false;
            }

            EnsureInitialized();

            directoryPath = directoryPath.Replace(Path.DirectorySeparatorChar, '/').TrimEnd('/') + '/';
            return _msixPackage.Entries.Any(e => e.FullName.StartsWith(directoryPath, StringComparison.OrdinalIgnoreCase));
        }

        public void Dispose()
        {
            if (_disposableStreams == null)
            {
                return;
            }

            foreach (var item in _disposableStreams)
            {
                item.Dispose();
            }
        }
        private void EnsureInitialized()
        {
            if (_msixPackage != null)
            {
                return;
            }

            if (!System.IO.File.Exists(_msixPackagePath))
            {
                throw new ArgumentException(string.Format(Resources.Localization.Packages_Error_FileMissing_Format, _msixPackagePath));
            }

            var fileStream = System.IO.File.OpenRead(_msixPackagePath);

            try
            {
                _msixPackage = new ZipArchive(fileStream, ZipArchiveMode.Read, false);
                _disposableStreams = new IDisposable[] { _msixPackage, fileStream };
            }
            catch (InvalidDataException e)
            {
                _disposableStreams = new IDisposable[] { fileStream };
                throw new InvalidDataException(Resources.Localization.Packages_Error_NotAPackage, e);
            }
            catch (Exception)
            {
                _disposableStreams = new IDisposable[] { fileStream };
                throw;
            }
        }
    }
}